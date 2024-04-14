using Sqids;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using WebDoomer.Zandronum;

namespace WebDoomerApi.Services;

// TODO: The pending data should copy itself over to the main data once enough servers exist for a given engine.
internal sealed class ConcurrentServerDataProvider : IServerDataProvider
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="SqidsEncoder{T}"/>
	private readonly SqidsEncoder<uint> _encoder;

	/// <summary>
	/// Represents the concurrent dictionary containing all data related to a specific's engine servers.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, ConcurrentBag<ServerResult>> _data;

	/// <summary>
	/// Represents the concurrent dictionary containing pending data that will replace <see cref="_data"/> once all servers were fetched.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, ConcurrentBag<ServerResult>> _pendingData;

	/// <summary>
	/// Represents a lazy loaded collection of servers.
	/// </summary>
	private Lazy<ReadOnlyCollection<ProvidedServer>> _servers;

	private ReadOnlyCollection<ProvidedServer> Servers => this._servers.Value;

	public ConcurrentServerDataProvider(
		ILogger<ConcurrentServerDataProvider> logger)
	{
		this._logger = logger;

		this._encoder = new();
		this._data = new();
		this._pendingData = new();
		this._servers = new(this.LazyGetServers);
	}

	/// <inheritdoc />
	void IServerDataProvider.StartSetData(EngineType engineType)
	{
		_ = this._pendingData.Remove(engineType, out _);
	}

	/// <inheritdoc />
	void IServerDataProvider.AddData(EngineType engineType, ServerResult serverResult)
	{
		var bag = this._pendingData.TryGetValue(engineType, out var outBag) ?
			outBag :
			new();

		_ = this._pendingData.TryAdd(engineType, bag);

		bag.Add(serverResult);
	}

	/// <inheritdoc />
	void IServerDataProvider.EndSetData(EngineType engineType)
	{
		if (!this._pendingData.TryGetValue(engineType, out var servers))
		{
			this._logger.LogError("Failed to copy server results over from pending data to actual data for {Engine}.", engineType);
			return;
		}

		_ = this._data.Remove(engineType, out _);
		_ = this._data.TryAdd(engineType, servers);

		// Reset lazy loaded servers.
		if (this._servers.IsValueCreated)
		{
			this._servers = new(this.LazyGetServers);
		}

		var completeCount = this._data[engineType].Count(x => x.State == ServerResultState.Success);
		var errorCount = this._data[engineType].Count(x => x.State == ServerResultState.Error);
		var timeoutCount = this._data[engineType].Count(x => x.State == ServerResultState.TimeOut);
		this._logger.LogInformation("Final collection for {Engine}: Complete count: {CompleteCount}. Error count: {ErrorCount}. Timeout count: {TimeoutCount}", engineType, completeCount, errorCount, timeoutCount);
	}

	/// <inheritdoc />
	public IEnumerable<ProvidedServer> GetServersRange(OrderByType orderBy, int skip, int take)
	{
		var servers = this.GetServers(orderBy);

		// Skip gets maxed to 0 so there's no negative result.
		// Take is clamped between 0 and a maximum value.
		skip = Math.Max(skip, 0);
		take = Math.Clamp(take, 0, 1000);

		var serverRange = servers
			.Skip(skip)
			.Take(take)
			.ToArray();

		return serverRange;
	}

	/// <inheritdoc />
	public ProvidedServer? GetServerByIdOrDefault(string id)
	{
		return this.Servers.SingleOrDefault(x => x.Id == id);
	}

	/// <inheritdoc />
	public IEnumerable<string> GetServerIds(OrderByType orderBy)
	{
		var servers = this.GetServers(orderBy);
		return servers.Select(x => x.Id);
	}

	private IEnumerable<ProvidedServer> GetServers(OrderByType orderBy)
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


	private ReadOnlyCollection<ProvidedServer> LazyGetServers()
	{
		var servers = this._data
			.SelectMany(x =>
				x.Value.Select(y => ProvidedServer.Create(y, x.Key, this._encoder)));

		return new(servers.ToArray());
	}
}
