using Microsoft.Extensions.Options;
using Sqids;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using WebDoomer.QZandronum;
using WebDoomer.Zandronum;

namespace WebDoomerApi.Services;

/// <summary>
/// Represents the base server data provider that handles data concurrently over different engines.
/// </summary>
internal sealed class ConcurrentServerDataProvider : IServerDataProvider, IDisposable
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="SqidsEncoder{T}"/>
	private readonly SqidsEncoder<uint> _encoder;

	/// <summary>
	/// Represents the concurrent dictionary containing all data related to a specific's engine servers.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, ConcurrentDictionary<IPEndPoint, ServerResult>> _data;

	/// <summary>
	/// Represents the concurrent dictionary containing pending data that will replace <see cref="_data"/> once enough servers have been added.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, ConcurrentDictionary<IPEndPoint, ServerResult>> _pendingData;

	/// <summary>
	/// Represents the expected number of servers to arrive for the engine.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, int> _expectedCount;

	/// <summary>
	/// Indicates per engine if the data should be written to the actual data dictionary.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, bool> _writeToActual;

	/// <summary>
	/// Represents a lazy loaded collection of servers.
	/// </summary>
	private Lazy<ReadOnlyCollection<ProvidedServer>> _servers;

	private ReadOnlyCollection<ProvidedServer> Servers => this._servers.Value;

	/// <summary>
	/// The service's options.
	/// </summary>
	private ApiOptions _options;

	/// <summary>
	/// The service's option listener.
	/// </summary>
	private readonly IDisposable? _optionsMonitorListener;

	public ConcurrentServerDataProvider(
		ILogger<ConcurrentServerDataProvider> logger,
		IOptionsMonitor<ApiOptions> optionsMonitor)
	{
		this._logger = logger;
		this._options = optionsMonitor.CurrentValue;
		this._optionsMonitorListener = optionsMonitor.OnChange(this.OptionsMonitorOnChangeListener);

		this._encoder = new();
		this._data = new();
		this._pendingData = new();
		this._expectedCount = new();
		this._writeToActual = new();
		this._servers = new(this.LazyGetServers);
	}

	/// <inheritdoc />
	void IServerDataProvider.StartSetData(EngineType engineType, int expectedCount)
	{
		if (this._expectedCount.ContainsKey(engineType))
		{
			this._logger.LogWarning("Tried starting new data set for {Engine} even though an existing start already exists.", engineType);
			return;
		}

		_ = this._pendingData.Remove(engineType, out _);
		this._expectedCount[engineType] = expectedCount;
		this._writeToActual[engineType] = false;

		this._logger.LogWarning("Start new data set for {EngineType}. Expected server count: {ExpectedCount}.", engineType, expectedCount);
	}

	/// <inheritdoc />
	void IServerDataProvider.AddData(EngineType engineType, ServerResult serverResult)
	{
		var dataDictionary = this._data.TryGetValue(engineType, out var outDataDictionary) ?
			outDataDictionary :
			new();

		var pendingDataDictionary = this._pendingData.TryGetValue(engineType, out var outPendingDataDictionary) ?
			outPendingDataDictionary :
			new();

		_ = this._data.TryAdd(engineType, dataDictionary);
		_ = this._pendingData.TryAdd(engineType, pendingDataDictionary);

		// Check actual data.
		// If none exists we should immediatley write to the actual data.
		if (!this._writeToActual[engineType] && dataDictionary.IsEmpty)
		{
			this._logger.LogDebug("Actual data dictionary of {EngineType} is empty. Switching to fill.", engineType);

			this._writeToActual[engineType] = true;
		}

		// Check if the pending dictionary should be switched to the actual dictionary.
		var targetPendingServerCount = this._expectedCount[engineType] * ((float)this._options.MinimumPendingServerPercentage / 100);
		if (!this._writeToActual[engineType] && pendingDataDictionary.Count > targetPendingServerCount)
		{
			this._logger.LogDebug("Pending dictionary reached threshold, {MinimumPercentage}% of {ExpectedCount} ({Actual}) for {EngineType}. Switching to actual data dictionary.", this._options.MinimumPendingServerPercentage, this._expectedCount[engineType], pendingDataDictionary.Count, engineType);

			this._writeToActual[engineType] = true;
			this._data[engineType] = this._pendingData[engineType];
			this._pendingData[engineType] = new();
		}

		// Get the dictionary 
		var dictionary = this._writeToActual[engineType] ?
			dataDictionary :
			pendingDataDictionary;

		if (dictionary.ContainsKey(serverResult.EndPoint))
		{
			this._logger.LogWarning("Concurrent dictionary already contains a result for endpoint {EndPoint}.", serverResult.EndPoint);
			return;
		}

		_ = dictionary.TryAdd(serverResult.EndPoint, serverResult);

		// Reset lazy loaded servers.
		if (this._servers.IsValueCreated)
		{
			this._servers = new(this.LazyGetServers);
		}
	}

	/// <inheritdoc />
	void IServerDataProvider.EndSetData(EngineType engineType)
	{
		_ = this._expectedCount.Remove(engineType, out _);
		_ = this._writeToActual.Remove(engineType, out _);

		if (!this._pendingData[engineType].IsEmpty)
		{
			this._logger.LogWarning("Pending server dictionary of {EngineType} is not empty.", engineType);
			this._pendingData[engineType].Clear();
		}

		var completeCount = this._data[engineType].Count(x => x.Value.State == ServerResultState.Success);
		var errorCount = this._data[engineType].Count(x => x.Value.State == ServerResultState.Error);
		var timeoutCount = this._data[engineType].Count(x => x.Value.State == ServerResultState.TimeOut);
		this._logger.LogInformation("Final collection for {Engine}: Complete count: {CompleteCount}. Error count: {ErrorCount}. Timeout count: {TimeoutCount}", engineType, completeCount, errorCount, timeoutCount);
	}

	/// <inheritdoc />
	public ProvidedServer? GetServerByIdOrDefault(string id)
	{
		return this.Servers.SingleOrDefault(x => x.Id == id);
	}

	/// <inheritdoc />
	public string? GetServerId(IPEndPoint search)
	{
		// Order type doesn't matter since a single server is expected.
		var servers = this.GetServersOrdered(OrderByType.PlayersDescending);
		servers = this.FilterServers(servers, search);

		var serversList = servers.ToList();
		Debug.Assert(serversList.Count <= 1);

		return serversList.FirstOrDefault()?.Id;
	}

	/// <inheritdoc />
	public IEnumerable<string> GetServerIds(string? search = null, OrderByType orderBy = OrderByType.PlayersDescending)
	{
		var servers = this.GetServersOrdered(orderBy);
		if (search != null)
		{
			servers = this.FilterServers(servers, search);
		}

		return servers.Select(x => x.Id);
	}

	/// <inheritdoc />
	public IEnumerable<string> GetServerIds(IPAddress search, OrderByType orderBy = OrderByType.PlayersDescending)
	{
		var servers = this.GetServersOrdered(orderBy);
		servers = this.FilterServers(servers, search);

		return servers.Select(x => x.Id);
	}

	/// <inheritdoc />
	public ProvidedServer? GetServersRange(IPEndPoint search)
	{
		// Order type doesn't matter since a single server is expected.
		var servers = this.GetServersOrdered(OrderByType.PlayersDescending);
		servers = this.FilterServers(servers, search);

		var serversList = servers.ToList();
		Debug.Assert(serversList.Count <= 1);

		return serversList.FirstOrDefault();
	}

	/// <inheritdoc />
	public IEnumerable<ProvidedServer> GetServersRange(string? search = null, int skip = 0, int take = 0, OrderByType orderBy = OrderByType.PlayersDescending)
	{
		var servers = this.GetServersOrdered(orderBy);

		if (search != null)
		{
			servers = this.FilterServers(servers, search);
		}

		servers = this.SkipTakeServers(servers, skip, take);
		return servers;
	}

	/// <inheritdoc />
	public IEnumerable<ProvidedServer> GetServersRange(IPAddress search, int skip = default, int take = default, OrderByType orderBy = OrderByType.PlayersDescending)
	{
		var servers = this.GetServersOrdered(orderBy);
		servers = this.FilterServers(servers, search);

		servers = this.SkipTakeServers(servers, skip, take);
		return servers;
	}

	private IEnumerable<ProvidedServer> GetServersOrdered(OrderByType orderBy)
	{
		var servers = this.Servers
			.OrderByDescending(x => x.Name == null ? 0 : 1);

		IEnumerable<ProvidedServer> result = orderBy switch
		{
			OrderByType.PlayersAscending => servers
				.OrderBy(x => (x.PlayingClientCount ?? 0) + (x.SpectatingClientCount ?? 0)),
			OrderByType.PlayersDescending => servers
				.OrderByDescending(x => (x.PlayingClientCount ?? 0) + (x.SpectatingClientCount ?? 0)),
			_ => this.Servers,
		};

		return result;
	}

	private IEnumerable<ProvidedServer> FilterServers(IEnumerable<ProvidedServer> servers, string search)
	{
		return servers.Where(x => x.Name != null && x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
	}

	private IEnumerable<ProvidedServer> FilterServers(IEnumerable<ProvidedServer> servers, IPAddress search)
	{
		return servers.Where(x => x.EndPoint.Address.Equals(search));
	}

	private IEnumerable<ProvidedServer> FilterServers(IEnumerable<ProvidedServer> servers, IPEndPoint search)
	{
		return servers.Where(x => x.EndPoint.Equals(search));
	}

	private IEnumerable<ProvidedServer> SkipTakeServers(IEnumerable<ProvidedServer> servers, int skip, int take)
	{
		if (take <= 0)
		{
			take = 100;
		}

		// Skip gets maxed to 0 so there's no negative result.
		// Take is clamped between 0 and a maximum value.
		skip = Math.Max(skip, 0);
		take = Math.Clamp(take, 0, 1000);

		return servers
			.Skip(skip)
			.Take(take);
	}

	private ReadOnlyCollection<ProvidedServer> LazyGetServers()
	{
		var servers = this._data
			.SelectMany(x =>
				x.Value.Select(y => ProvidedServer.Create(y.Value, x.Key, this._encoder)));

		return new(servers.ToArray());
	}

	private void OptionsMonitorOnChangeListener(ApiOptions options, string? _)
	{
		this._logger.LogDebug("Settings update observed.");
		this._options = options;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this._optionsMonitorListener?.Dispose();
	}
}
