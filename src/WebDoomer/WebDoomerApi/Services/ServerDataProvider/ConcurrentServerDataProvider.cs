using Sqids;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using WebDoomer.Zandronum;
using WebDoomerApi.Controllers;

namespace WebDoomerApi.Services;

internal sealed class ConcurrentServerDataProvider : IServerDataProvider
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="SqidsEncoder{T}"/>
	private readonly SqidsEncoder<uint> _encoder;

	/// <summary>
	/// Represents the concurrent dictionary containing all data related to a specific's engine servers.
	/// </summary>
	private readonly ConcurrentDictionary<EngineType, IDictionary<IPAddress, ConcurrentBag<ServerResult>>> _engineData;

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
		this._engineData = new();
		this._servers = new(this.LazyGetServers);
	}

	/// <inheritdoc />
	void IServerDataProvider.SetData(EngineType engineType, IDictionary<IPAddress, ConcurrentBag<ServerResult>> data)
	{
		_ = this._engineData.Remove(engineType, out _);
		_ = this._engineData.TryAdd(engineType, data);

		if(this._servers.IsValueCreated)
		{
			this._servers = new(this.LazyGetServers);
		}
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
		return new(this.LazyGetServersEnumerable().ToArray());
	}

	private IEnumerable<ProvidedServer> LazyGetServersEnumerable()
	{
		foreach (var (engine, engineResults) in this._engineData)
		{
			foreach(var serverResults in engineResults.Values)
			{
				foreach(var serverResult in serverResults)
				{
					yield return ProvidedServer.Create(serverResult, engine, this._encoder);
				}
			}
		}
	}
}
