using System.Diagnostics;
using System.Text;
using WebDoomerApi.Scheduling;

namespace WebDoomerTests;

public class ScheduledEventBuilderTests
{
	private sealed class TestJob : IAsyncScheduledInvoke
	{
		public Task InvokeAsync(CancellationToken _)
		{
			return Task.CompletedTask;
		}
	}

	[Fact]
    public void ScheduledEventBuilderBuildsScheduledEventWithMatchingParameters()
    {
		var scheduledEventBuilder = ScheduledEventBuilder.Create<TestJob>("Test", TimeSpan.FromSeconds(1));
		var scheduledEvent = scheduledEventBuilder.Build();
		Assert.Equal("Test", scheduledEvent.Key);
		Assert.Equal(TimeSpan.FromSeconds(1), scheduledEvent.Interval);
		Assert.Equal(typeof(TestJob), scheduledEvent.InvokableType);
	}
}