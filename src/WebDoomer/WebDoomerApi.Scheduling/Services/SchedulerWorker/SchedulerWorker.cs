using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WebDoomerApi.Scheduling;

internal sealed class SchedulerWorker : IResettable
{
	public Action<SchedulerWorker>? OnFinished { get; private set; }
	public Action<SchedulerWorker, Exception>? OnFailOnce { get; private set; }
	public Action<SchedulerWorker, Exception>? OnFailure { get; private set; }
	public ScheduledEventContext? ScheduledEventContext { get; private set; }

	public SchedulerWorker()
	{
	}

	/// <summary>
	/// Invokes the worker using the given schedules.
	/// </summary>
	/// <param name="onFinished">The callback to invoke on completion.</param>
	public void Invoke(Action<SchedulerWorker> onFinished, Action<SchedulerWorker, Exception> onFailOnce, Action<SchedulerWorker, Exception> onFailure, ScheduledEventContext scheduledEventContext, CancellationToken cancellationToken)
	{
		this.OnFinished = onFinished;
		this.OnFailOnce = onFailOnce;
		this.OnFailure = onFailure;
		this.ScheduledEventContext = scheduledEventContext;

		// Check for dependency.
		if (scheduledEventContext.JobDependency is not IAsyncScheduledInvoke asyncScheduledInvoke)
		{
			var type = scheduledEventContext.ScheduledEvent.InvokableType;
			var exception = new ArgumentException($"Scheduled invokable type {type.Name} was either not found or does not implement {nameof(IAsyncScheduledInvoke)}");
			this.OnFailure(this, exception);
			return;
		}

		this.DoInvokeSchedule(cancellationToken);
	}

	private async void DoInvokeSchedule(CancellationToken cancellationToken)
	{
		Debug.Assert(this.OnFinished != null);
		Debug.Assert(this.OnFailOnce != null);
		Debug.Assert(this.OnFailure != null);
		Debug.Assert(this.ScheduledEventContext?.JobDependency is IAsyncScheduledInvoke);

		var asyncScheduledInvoke = this.ScheduledEventContext.JobDependency as IAsyncScheduledInvoke;
		var totalRetries = this.ScheduledEventContext.ScheduledEvent.RetryCount ?? 0;
		while (true)
		{
			Exception? exception;
			try
			{
				await asyncScheduledInvoke!.InvokeAsync(cancellationToken)
					.ConfigureAwait(false);

				exception = null;
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			// There was an error.
			// Increase retry count and retry.
			if (exception != null)
			{
				// Total retry count reached.
				if (totalRetries - this.ScheduledEventContext.Retries == 0)
				{
					this.OnFailure(this, exception);
					return;
				}

				this.OnFailOnce(this, exception);
				this.ScheduledEventContext.Retries++;

				var interval = this.ScheduledEventContext.ScheduledEvent.RetryInterval;
				Debug.Assert(interval.HasValue);

				await Task.Delay(interval.Value, cancellationToken)
					.ConfigureAwait(false);
				continue;
			}

			break;
		}

		// The worker succesfully invoked the event.
		this.OnFinished.Invoke(this);
	}

	/// <inheritdoc />
	public bool TryReset()
	{
		this.OnFinished = null;
		this.OnFailOnce = null;
		this.OnFailure = null;
		this.ScheduledEventContext = null;
		return true;
	}
}
