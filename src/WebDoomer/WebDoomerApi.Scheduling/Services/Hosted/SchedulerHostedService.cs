using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WebDoomerApi.Scheduling;

internal sealed class SchedulerHostedService : IHostedService, IDisposable
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="IHostApplicationLifetime"/>
	private readonly IHostApplicationLifetime _hostApplicationLifetime;

	/// <inheritdoc cref="IScheduler"/>
	private readonly IScheduler _scheduler;

	/// <summary>
	/// Represents the timer that is responsible for ticking the main scheduler every second.
	/// </summary>
	private Timer? _timer;

	/// <summary>
	/// Represents the source for the cancellation token to pass to the scheduler for invoked scheduled tasks.
	/// </summary>
	private CancellationTokenSource? _schedulerCancellationTokenSource;

	public SchedulerHostedService(
		ILogger<SchedulerHostedService> logger,
		IHostApplicationLifetime hostApplicationLifetime,
		IScheduler scheduler)
	{
		this._logger = logger;
		this._hostApplicationLifetime = hostApplicationLifetime;
		this._scheduler = scheduler;
	}

	/// <inheritdoc />
	public Task StartAsync(CancellationToken cancellationToken)
	{
		// This is mostly added because I noticed Coravel implemented this.
		// Apparently running `StartAsync` is prone to issues where the DI container is not fully configured causing silent errors due to missing services.
		// See: https://github.com/jamesmh/coravel/pull/259
		_ = this._hostApplicationLifetime.ApplicationStarted.Register(this.OnApplicationStarted);
		return Task.CompletedTask;
	}

	private void OnApplicationStarted()
	{
		this._logger.LogDebug("Scheduler hosted service is starting.");
		this._timer = new(this.TimerRunUserSyncInvokerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
		this._schedulerCancellationTokenSource = new();
	}

	/// <summary>
	/// The callback that is triggered by the timer when it elapses. The function will call the main invoker to trigger the scheduler.
	/// </summary>
	private void TimerRunUserSyncInvokerCallback(object? _)
	{
		Debug.Assert(this._schedulerCancellationTokenSource != null);
		this._scheduler.Tick(this._schedulerCancellationTokenSource.Token);
	}

	/// <inheritdoc />
	public async Task StopAsync(CancellationToken cancellationToken)
	{
		Debug.Assert(this._schedulerCancellationTokenSource != null);

		// By setting the timer to last infinite we effectively stop it until it's disposed.
		// (FYI, -1 is allowed and overflows to int.max when the value is cast to an unsigned integer internally).
		_ = this._timer?.Change(-1, 0);

		// Call cancellation on any scheduled tasks that run.
		await this._schedulerCancellationTokenSource.CancelAsync()
			.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this._timer?.Dispose();
		this._schedulerCancellationTokenSource?.Dispose();
	}
}
