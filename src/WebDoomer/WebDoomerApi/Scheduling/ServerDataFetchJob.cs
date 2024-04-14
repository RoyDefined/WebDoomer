using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

		var zandronumServerEnumerable = this.DoInvokeForEngineAsync(this._zandronumMasterServerService, this._zandronumServerService, "master.zandronum.com", 15300, cancellationToken);
		var qZandronumServerEnumerable = this.DoInvokeForEngineAsync(this._qZandronumMasterServerService, this._qZandronumServerService, "master.qzandronum.com", 15300, cancellationToken);

		// Delegate to a function that awaits the result and stores the result in the provider.
		var zandronumServerFetchTask = this.AwaitFetchAndSave(zandronumServerEnumerable, EngineType.Zandronum);
		var qZandronumServerFetchTask = this.AwaitFetchAndSave(qZandronumServerEnumerable, EngineType.QZandronum);

		// Await the tasks so the job gracefully ends and allows the scheduler to correctly set the next invoke.
		var task = Task.WhenAll([zandronumServerFetchTask, qZandronumServerFetchTask]);
		await task;

		this._logger.LogDebug("Server data fetch job finished.");
	}

	// This is the main invoke of the job, using the services required.
	// The services can be Zandronum or QZandronum. QZandronum inherits from Zandronum as it's basically the same.
	private async IAsyncEnumerable<ServerResult> DoInvokeForEngineAsync(IZandronumMasterServerService masterServerService, IZandronumServerService serverService, string address, int port, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Result servers from master server {Address}:{Port}.", address, port);
		var masterResult = await masterServerService.GetMasterServerHostsAsync(address, port, cancellationToken);

		// No result
		if (masterResult.ServerChallengeResponse != ServerChallengeResponseType.beginServerListPart)
		{
			this._logger.LogWarning("Master server {Address}:{Port} returned non-positive response.", address, port);
			yield break;
		}

		var endPoints = masterResult.Hosts
			.SelectMany(x => x.Ports.Select(y => new IPEndPoint(x.Address, y)))
			.ToArray();

		this._logger.LogInformation("Start fetching from {Address}:{Port}. Total endpoints: {EndPoints}.", address, port, endPoints.Length);
		var resultsEnumerable = serverService.GetServersDataAsync(endPoints, LauncherProtocolType.OldProtocolSegmented, ServerQueryDataFlagset0.all, ServerQueryDataFlagset1.all, cancellationToken);

		await foreach (var result in resultsEnumerable)
		{
			yield return result;
		}
	}

	private async Task AwaitFetchAndSave(IAsyncEnumerable<ServerResult> asyncEnumerable, EngineType engineType)
	{
		this._serverDataProvider.StartSetData(engineType);

		try
		{
			await foreach(var serverResult in asyncEnumerable)
			{
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
