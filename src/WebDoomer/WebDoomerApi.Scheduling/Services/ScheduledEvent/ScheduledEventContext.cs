using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace WebDoomerApi.Scheduling;

internal sealed class ScheduledEventContext
{
	/// <summary>
	/// Represents the target schedule to use.
	/// </summary>
	public ScheduledEvent ScheduledEvent { get; }

	/// <summary>
	/// Represents the next date time at which the included schedule should be invoked.
	/// </summary>
	public DateTimeOffset? NextInvoke { get; private set; }

	/// <summary>
	/// Represents the job dependency to call when the worker invokes the job.
	/// </summary>
	public object? JobDependency { get; internal set; }

	/// <summary>
	/// Represents the state of the scheduled event.
	/// </summary>
	public ScheduledEventState State { get; internal set; }

	/// <summary>
	/// Represents the number of retries that have been done during invokation of the job.
	/// </summary>
	public int Retries { get; internal set; }

	public ScheduledEventContext(
		ScheduledEvent scheduledEvent)
	{
		this.ScheduledEvent = scheduledEvent;
	}

	/// <summary>
	/// Sets the initial invoke <see cref="DateTime"/> for this context based on the given <see cref="ScheduledEvent.StartInterval"/>.<br/>
	/// If the <see cref="ScheduledEvent.StartInterval"/> was not set the context will instead use the <see cref="ScheduledEvent.Interval"/>.
	/// </summary>
	/// <param name="timeProvider">The time provider to provide the current UTC time.</param>
	public void SetInitialInvoke(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));

		var interval = this.ScheduledEvent.StartInterval ?? this.ScheduledEvent.Interval;
		this.NextInvoke = timeProvider.GetUtcNow()
			.Add(interval);
	}

	/// <summary>
	/// Sets the next invoke <see cref="DateTime"/> for this context based on the given <see cref="ScheduledEvent.Interval"/>.
	/// </summary>
	/// <param name="timeProvider">The time provider to provide the current UTC time.</param>
	public void UpdateNextInvoke(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
		this.NextInvoke = timeProvider.GetUtcNow().Add(this.ScheduledEvent.Interval);
	}

	/// <summary>
	/// Returns <see langword="true"/> if the current scheduled event is due.
	/// </summary>
	/// <param name="timeProvider">The time provider to provide the current UTC time.</param>
	/// <returns><see langword="true"/> if due, else <see langword="false"/>.</returns>
	public bool GetIsDue(TimeProvider timeProvider)
	{
		Debug.Assert(this.NextInvoke != null);

		ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
		return this.NextInvoke < timeProvider.GetUtcNow();
	}
}