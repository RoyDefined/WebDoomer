namespace WebDoomerApi.Scheduling;

/// <summary>
/// Represents the details of a scheduled event.
/// </summary>
public readonly struct ScheduledEvent : IEquatable<ScheduledEvent>
{
	/// <summary>
	/// The main key of the scheduled event.
	/// </summary>
	public required string Key { get; init; }

	/// <summary>
	/// The invokation interval that should be used by the event.
	/// </summary>
	public required TimeSpan Interval { get; init; }

	/// <summary>
	/// The dependency type of the event that should be invoked.
	/// </summary>
	public required Type InvokableType { get; init; }

	/// <summary>
	/// The initial start interval that should be used by the event when it is created.<br/>
	/// If the value is <see langword="null"/> the scheduler will use the <see cref="Interval"/> instead.
	/// </summary>
	public required TimeSpan? StartInterval { get; init; }

	/// <summary>
	/// The number of retries a worker should do in the event a scheduled event fails to invoke.<br/>
	/// If the value is <see langword="null"/> no retry policy was configured to run.
	/// </summary>
	public required int? RetryCount { get; init; }

	/// <summary>
	/// The retry interval which should be waited on when invokation fails and a retry is triggered.<br/>
	/// If the value is <see langword="null"/> no retry policy was configured to run.
	/// </summary>
	public required TimeSpan? RetryInterval { get; init; }

	/// <inheritdoc />
	public readonly bool Equals(ScheduledEvent other)
	{
		return other.Key.Equals(this.Key, StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc />
	public readonly override bool Equals(object? obj)
	{
		return obj is ScheduledEvent scheduledEvent &&
			this.Equals(scheduledEvent);
	}

	/// <inheritdoc />
	public readonly override int GetHashCode()
	{
		return this.Key.GetHashCode(StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Compares the two <see cref="ScheduledEvent"/> instances for equality.
	/// </summary>
	/// <param name="left">A <see cref="ScheduledEvent"/> to compare.</param>
	/// <param name="right">A <see cref="ScheduledEvent"/> to compare.</param>
	/// <returns><see langword="true"/> if the two instances match, otherwise <see langword="false"/>.</returns>
	public static bool operator ==(ScheduledEvent left, ScheduledEvent right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares the two <see cref="ScheduledEvent"/> instances for inequality.
	/// </summary>
	/// <param name="left">A <see cref="ScheduledEvent"/> to compare.</param>
	/// <param name="right">A <see cref="ScheduledEvent"/> to compare.</param>
	/// <returns><see langword="true"/> if the two instances do not match, otherwise <see langword="false"/>.</returns>
	public static bool operator !=(ScheduledEvent left, ScheduledEvent right)
	{
		return !(left == right);
	}
}