using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Diagnostics;
using System.Threading;

namespace WebDoomerApi.Scheduling;

internal sealed class Scheduler : IScheduler
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="IServiceProvider"/>
	private readonly IServiceProvider _serviceProvider;

	/// <inheritdoc cref="ObjectPool{T}"/>
	private readonly ObjectPool<SchedulerWorker> _workerPool;

	/// <inheritdoc cref="TimeProvider"/>
	private readonly TimeProvider _timeProvider;

	/// <summary>
	/// Represents a list contexts containing scheduled events to invoke.
	/// </summary>
	private readonly List<ScheduledEventContext> _scheduledEventContexts;

	public Scheduler(
		ILogger<Scheduler> logger,
		IServiceProvider serviceProvider,
		ObjectPool<SchedulerWorker> workerPool,
		TimeProvider timeProvider)
	{
		this._logger = logger;
		this._serviceProvider = serviceProvider;
		this._workerPool = workerPool;
		this._timeProvider = timeProvider;
		this._scheduledEventContexts = new();
	}

	/// <inheritdoc />
	void IScheduler.Tick(CancellationToken cancellationToken)
	{
		foreach (var scheduleContext in this._scheduledEventContexts)
		{
			if (scheduleContext.State == ScheduledEventState.running)
			{
				continue;
			}

			if (!scheduleContext.GetIsDue(this._timeProvider))
			{
				continue;
			}

			this.InvokeWorker(scheduleContext, cancellationToken);
		}
	}

	/// <inheritdoc />
	public void AddSchedule(ScheduledEvent scheduledEvent)
	{
		var context = new ScheduledEventContext(scheduledEvent);
		context.SetInitialInvoke(this._timeProvider);
		this._scheduledEventContexts.Add(context);

		Debug.Assert(context.NextInvoke.HasValue);
		this._logger.LogInformation("Added scheduled event {Id}. First invoke will be at {InvokeDateTime}", context.ScheduledEvent.Key, context.NextInvoke.Value);
	}

	private void InvokeWorker(ScheduledEventContext context, CancellationToken cancellationToken)
	{
		Debug.Assert(context.State == ScheduledEventState.available);
		context.State = ScheduledEventState.running;

		this._logger.LogDebug("Invoking worker for schedule {ScheduleKey}.", context.ScheduledEvent.Key);

		// Insert the dependency job.
		context.JobDependency ??= this._serviceProvider.GetService(context.ScheduledEvent.InvokableType);

		var worker = this._workerPool.Get();
		worker.Invoke(this.OnWorkerFinished, this.OnWorkerFailOnce, this.OnWorkerFailure, context, cancellationToken);
	}

	private void OnWorkerFinished(SchedulerWorker worker)
	{
		Debug.Assert(worker.ScheduledEventContext != null);
		Debug.Assert(worker.ScheduledEventContext.State == ScheduledEventState.running);

		this._logger.LogDebug("Schedule finished: {ScheduleKey}.", worker.ScheduledEventContext.ScheduledEvent.Key);
		this.FinalizeWorker(worker);
	}

	private void OnWorkerFailOnce(SchedulerWorker worker, Exception ex)
	{
		Debug.Assert(worker?.ScheduledEventContext != null);
		Debug.Assert(worker.ScheduledEventContext.State == ScheduledEventState.running);

		this._logger.LogError(ex, "Scheduled event failed to execute: {ScheduleKey} (try {Retries}/{TotalRetries}).", worker.ScheduledEventContext.ScheduledEvent.Key, worker.ScheduledEventContext.Retries + 1, worker.ScheduledEventContext.ScheduledEvent.RetryCount);
	}

	private void OnWorkerFailure(SchedulerWorker worker, Exception ex)
	{
		Debug.Assert(worker?.ScheduledEventContext != null);
		Debug.Assert(worker.ScheduledEventContext.State == ScheduledEventState.running);

		this._logger.LogError(ex, "Schedule finished with error: {ScheduleKey}.", worker.ScheduledEventContext.ScheduledEvent.Key);
		this.FinalizeWorker(worker);
	}

	private void FinalizeWorker(SchedulerWorker worker)
	{
		Debug.Assert(worker?.ScheduledEventContext != null);

		worker.ScheduledEventContext.Retries = 0;
		worker.ScheduledEventContext.State = ScheduledEventState.available;
		worker.ScheduledEventContext.UpdateNextInvoke(this._timeProvider);

		Debug.Assert(worker.ScheduledEventContext.NextInvoke.HasValue);
		this._logger.LogInformation("Scheduled event {Id} will invoke next at {InvokeDateTime}", worker.ScheduledEventContext.ScheduledEvent.Key, worker.ScheduledEventContext.NextInvoke.Value);

		this._workerPool.Return(worker);
	}
}
