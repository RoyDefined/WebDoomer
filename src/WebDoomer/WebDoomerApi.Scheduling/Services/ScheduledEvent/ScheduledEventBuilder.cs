namespace WebDoomerApi.Scheduling;

/// <summary>
/// Represents the main builder for a scheduled event.<br/>
/// The builder allows for creations of new events using a Fluent API design pattern.
/// </summary>
public sealed class ScheduledEventBuilder
{
	internal readonly string key;
	internal readonly TimeSpan interval;
	internal readonly Type invokableType;

	internal TimeSpan? startInterval;

	internal int? retryCount;
	internal TimeSpan? retryInterval;

	private ScheduledEventBuilder(
		string key,
		TimeSpan interval,
		Type invokableType,
		int retryCount)
	{
		this.key = key;
		this.interval = interval;
		this.invokableType = invokableType;
		this.retryCount = retryCount;
	}

	/// <summary>
	/// Builds the current context of a schedule into a <see cref="ScheduledEvent"/>.
	/// </summary>
	/// <returns>A <see cref="ScheduledEvent"/> instance.</returns>
	public ScheduledEvent Build()
	{
		return new ScheduledEvent()
		{
			Key = this.key,
			Interval = this.interval,
			InvokableType = this.invokableType,
			StartInterval = this.startInterval,
			RetryCount = this.retryCount,
			RetryInterval = this.retryInterval,
		};
	}

	/// <summary>
	/// Specifies the retry policy that the main worker should use in the event a scheduled event fails.
	/// </summary>
	/// <param name="count">The number of times the worker should retry invokation of the event.</param>
	/// <param name="interval">The interval between invokes.</param>
	/// <returns>The builder.</returns>
	public ScheduledEventBuilder WithRetry(int count, TimeSpan interval)
	{
		this.retryCount = count;
		this.retryInterval = interval;
		return this;
	}

	/// <summary>
	/// Specifies the scheduled event should start immediatley when it is added to the active scheduler.
	/// </summary>
	/// <returns>The builder.</returns>
	public ScheduledEventBuilder StartImmediatley()
	{
		return this.StartAfter(TimeSpan.Zero);
	}

	/// <summary>
	/// Specifies the time the scheduler should initially wait before starting.<br/>
	/// After this initial time has passed and the scheduler invoked, the regular interval will be used.
	/// </summary>
	/// <returns>The builder.</returns>
	public ScheduledEventBuilder StartAfter(TimeSpan time)
	{
		this.startInterval = time;
		return this;
	}

	/// <summary>
	/// Creates a new instance of <see cref="ScheduledEventBuilder"/> for building a <see cref="ScheduledEvent"/>.
	/// </summary>
	/// <typeparam name="T">A class implementing <see cref="IAsyncScheduledInvoke"/> to use for invokation of the event when the schedule triggers.</typeparam>
	/// <param name="key">A unique key provided to the schedule.</param>
	/// <param name="interval">The interval the schedule should invoke at.</param>
	/// <returns>The builder.</returns>
	public static ScheduledEventBuilder Create<T>(string key, TimeSpan interval)
		where T : IAsyncScheduledInvoke
	{
		return new(key, interval, typeof(T), 0);
	}
}
