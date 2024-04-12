using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using WebDoomer.QZandronum;
using WebDoomer.Zandronum;
using WebDoomerApi.Scheduling;
using WebDoomerApi.Services;

namespace WebDoomerApi.Jobs;

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

		var zandronumServerFetchTask = this.DoInvokeForEngineAsync(this._zandronumMasterServerService, this._zandronumServerService, "master.zandronum.com", 15300, cancellationToken);
		var qZandronumServerFetchTask = this.DoInvokeForEngineAsync(this._qZandronumMasterServerService, this._qZandronumServerService, "master.qzandronum.com", 15300, cancellationToken);

		Debug.Assert(zandronumServerFetchTask != null);
		Debug.Assert(qZandronumServerFetchTask != null);

		// Delegate to a function that awaits the result and stores the result in the provider.
		this.AwaitFetchAndSave(zandronumServerFetchTask, EngineType.Zandronum);
		this.AwaitFetchAndSave(qZandronumServerFetchTask, EngineType.QZandronum);

		// Await the tasks so the job gracefully ends and allows the scheduler to correctly set the next invoke.
		var tasks = Task.WhenAll([zandronumServerFetchTask, qZandronumServerFetchTask]);
		_ = await tasks;

		this._logger.LogDebug("Server data fetch job finished.");
	}

	// This is the main invoke of the job, using the services required.
	// The services can be Zandronum or QZandronum. QZandronum inherits from Zandronum as it's basically the same.
	private async Task<ConcurrentDictionary<IPAddress, ServerResult[]>?> DoInvokeForEngineAsync(IZandronumMasterServerService masterServerService, IZandronumServerService serverService, string address, int port, CancellationToken cancellationToken)
	{
		var masterResult = await masterServerService.GetMasterServerHostsAsync(address, port, cancellationToken);

		// No result
		if (masterResult.ServerChallengeResponse != ServerChallengeResponseType.beginServerListPart)
		{
			return null;
		}

		var concurrentDictionary = new ConcurrentDictionary<IPAddress, ServerResult[]>();
		this._logger.LogInformation("Start fetching from {Address}:{Port}.", address, port);

		// Fetch hosts in parallel.
		await Parallel.ForEachAsync(masterResult.Hosts, cancellationToken,
			async (host, cancellationToken) =>
			{
				var results = await this.DoInvokeForHostAsync(serverService, host, cancellationToken);
				_ = concurrentDictionary.TryAdd(host.Address, results);
			});

		this._logger.LogInformation("Finished fetching from {Address}:{Port}. Final count: {Index}", address, port, concurrentDictionary.Sum(x => x.Value.Length));
		return concurrentDictionary;
	}

	// Fetches the main data from the servers under the given host.
	// Returns an array of results.
	private async Task<ServerResult[]> DoInvokeForHostAsync(IZandronumServerService serverService, HostIdentification host, CancellationToken cancellationToken)
	{
		var results = new ServerResult[host.Ports.Count];
		var resultInterlockedIndex = -1;

		// Fetch server data in parallel.
		await Parallel.ForEachAsync(host.Ports, cancellationToken,
			async (port, cancellation) =>
			{
				var result = await serverService.GetServerDataAsync(host.Address, port, LauncherProtocolType.OldProtocolSegmented, ServerQueryDataFlagset0.all, ServerQueryDataFlagset1.all, cancellationToken);
				var index = Interlocked.Increment(ref resultInterlockedIndex);
				results[index] = result;
			});

		Array.Resize(ref results, resultInterlockedIndex + 1);
		return results;
	}

	private async void AwaitFetchAndSave(Task<ConcurrentDictionary<IPAddress, ServerResult[]>?> task, EngineType engineType)
	{
		var result = await task;
		if (result == null)
		{
			return;
		}

		this._serverDataProvider.SetData(engineType, result);
	}

	public static void Register(WebApplication app)
	{
		var schedule = ScheduledEventBuilder.Create<ServerDataFetchJob>(nameof(ServerDataFetchJob), TimeSpan.FromMinutes(1))
			.StartImmediatley()
			.WithRetry(3, TimeSpan.FromSeconds(5))
			.Build();

		var scheduler = app.Services.GetRequiredService<IScheduler>();
		scheduler.AddSchedule(schedule);
	}
}