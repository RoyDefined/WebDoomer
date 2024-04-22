using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using WebDoomer.QZandronum;
using WebDoomer.Zandronum;
using WebDoomerApi.Scheduling;
using WebDoomerApi.Services;

namespace WebDoomerApi.Jobs;

/// <summary>
/// Represents the main job that fetches all server data and stores them into a main provider.
/// </summary>
internal sealed class ServerDataFetchJob : IAsyncScheduledInvoke
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="IZandronumMasterServerService"/>
	private readonly IZandronumMasterServerService _zandronumMasterServerService;

	/// <inheritdoc cref="IZandronumServerService"/>
	private readonly IZandronumServerService _zandronumServerService;

	/// <inheritdoc cref="IQZandronumMasterServerService"/>
	private readonly IQZandronumMasterServerService _qZandronumMasterServerService;

	/// <inheritdoc cref="IQZandronumServerService"/>
	private readonly IQZandronumServerService _qZandronumServerService;

	/// <inheritdoc cref="IServerDataProvider"/>
	private readonly IServerDataProvider _serverDataProvider;

	public ServerDataFetchJob(
		ILogger<ServerDataFetchJob> logger,
		IZandronumMasterServerService zandronumMasterServerService,
		IZandronumServerService zandronumServerService,
		IQZandronumMasterServerService qZandronumMasterServerService,
		IQZandronumServerService qZandronumServerService,
		IServerDataProvider serverDataProvider)
	{
		this._logger = logger;
		this._zandronumMasterServerService = zandronumMasterServerService;
		this._zandronumServerService = zandronumServerService;
		this._qZandronumMasterServerService = qZandronumMasterServerService;
		this._qZandronumServerService = qZandronumServerService;
		this._serverDataProvider = serverDataProvider;
	}

	/// <inheritdoc />
	public async Task InvokeAsync(CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Server data fetch job invoked.");

		var zandronunEndPoints = await this.FetchEndpointsAsync(this._zandronumMasterServerService, "master.zandronum.com", 15300, cancellationToken);
		var qZandronunEndPoints = await this.FetchEndpointsAsync(this._qZandronumMasterServerService, "master.qzandronum.com", 15300, cancellationToken);

		var zandronumServerEnumerable = this._zandronumServerService.GetServersDataAsync(zandronunEndPoints, LauncherProtocolType.OldProtocolSegmented, ServerQueryDataFlagset0.all, ServerQueryDataFlagset1.all, cancellationToken);
		var qZandronumServerEnumerable = this._qZandronumServerService.GetServersDataAsync(qZandronunEndPoints, LauncherProtocolType.OldProtocolSegmented, ServerQueryDataFlagset0.all, ServerQueryDataFlagset1.all, cancellationToken);

		// Delegate to a function that awaits the result and stores the result in the provider.
		var zandronumServerFetchTask = this.AwaitFetchAndSave(zandronumServerEnumerable, EngineType.Zandronum, zandronunEndPoints.Length, cancellationToken);
		var qZandronumServerFetchTask = this.AwaitFetchAndSave(qZandronumServerEnumerable, EngineType.QZandronum, qZandronunEndPoints.Length, cancellationToken);

		// Await the tasks so the job gracefully ends and allows the scheduler to correctly set the next invoke.
		var task = Task.WhenAll([zandronumServerFetchTask, qZandronumServerFetchTask]);
		await task;

		this._logger.LogDebug("Server data fetch job finished.");
	}

	/// <summary>
	/// Handles fetching all server endpoints from the given <paramref name="masterServerService"/>.
	/// </summary>
	/// <param name="masterServerService">The service to handle the request.</param>
	/// <param name="address">The address under which to fetch the end points from.</param>
	/// <param name="port">The port under which to fetch the end points from.</param>
	/// <param name="cancellationToken">A token to cancel the operation early.</param>
	/// <returns>An awaitable task that returns an <see cref="Array"/> of <see cref="IPEndPoint"/> representing the collection of servers to fetch data from.</returns>
	private async Task<IPEndPoint[]> FetchEndpointsAsync(IZandronumMasterServerService masterServerService, string address, int port, CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Fetching servers from master server {Address}:{Port}.", address, port);
		var masterResult = await masterServerService.GetMasterServerHostsAsync(address, port, cancellationToken);

		// No result
		if (masterResult.ServerChallengeResponse != ServerChallengeResponseType.beginServerListPart)
		{
			this._logger.LogWarning("Master server {Address}:{Port} returned non-positive response.", address, port);
			return Array.Empty<IPEndPoint>();
		}

		var endPoints = masterResult.Hosts
			.SelectMany(x => x.Ports.Select(y => new IPEndPoint(x.Address, y)))
			.ToArray();

		return endPoints;
	}

	/// <summary>
	/// Handles awaiting the given <paramref name="asyncEnumerable"/> for incoming servers and saves them in the provider under the given <paramref name="engineType"/>.
	/// </summary>
	/// <param name="asyncEnumerable">The enumerable that must be awaited and handled.</param>
	/// <param name="engineType">The engine type under which the servers must be saved.</param>
	/// <param name="expectedServerCount">The expected number of servers that will be fetched from the <paramref name="asyncEnumerable"/>.</param>
	/// <param name="cancellationToken">A token to cancel the operation early.</param>
	/// <returns>An awaitable task.</returns>
	private async Task AwaitFetchAndSave(IAsyncEnumerable<ServerResult> asyncEnumerable, EngineType engineType, int expectedServerCount, CancellationToken cancellationToken)
	{
		this._serverDataProvider.StartSetData(engineType, expectedServerCount);

		try
		{
			await foreach(var serverResult in asyncEnumerable)
			{
				cancellationToken.ThrowIfCancellationRequested();
				this._serverDataProvider.AddData(engineType, serverResult);
			}
		}
		catch (Exception ex)
		{
			this._logger.LogError(ex, "There was an error fetching all servers for {Engine}", engineType);
		}
		finally
		{
			this._serverDataProvider.EndSetData(engineType);
		}
	}

	/// <summary>
	/// Registers an event to handle the <see cref="ServerDataFetchJob"/>.
	/// </summary>
	/// <param name="app">The web application pipeline to fetch dependencies from.</param>
	internal static void Register(WebApplication app)
	{
		var schedule = ScheduledEventBuilder.Create<ServerDataFetchJob>(nameof(ServerDataFetchJob), TimeSpan.FromMinutes(1))
			.StartImmediatley()
			.WithRetry(3, TimeSpan.FromSeconds(5))
			.Build();

		var scheduler = app.Services.GetRequiredService<IScheduler>();
		scheduler.AddSchedule(schedule);
	}
}
