namespace WebDoomerApi.Scheduling;

/// <summary>
/// Represents the base interface that scheduled events use to indicate they use the awaitable pattern to invoke their job.
/// </summary>
public interface IAsyncScheduledInvoke
{
	/// <summary>
	/// Asynchronously invokes the scheduled job.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token that will invoke on application shutdown to indicate graceful shutdown.</param>
	/// <returns>An awaitable task that will be used by the main schedule worker to await the response.</returns>
	Task InvokeAsync(CancellationToken cancellationToken);
}