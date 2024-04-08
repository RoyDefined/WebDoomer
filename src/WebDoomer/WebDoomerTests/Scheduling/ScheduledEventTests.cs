using System.Diagnostics;
using System.Text;
using WebDoomerApi.Scheduling;

namespace WebDoomerTests;

public class ScheduledEventTests
{
	private readonly ScheduledEvent _eventLeftEqual;
	private readonly ScheduledEvent _eventRightEqual;

	private readonly ScheduledEvent _eventLeftNotEqual;
	private readonly ScheduledEvent _eventRightNotEqual;

	public ScheduledEventTests()
	{
		this._eventLeftEqual = new ScheduledEvent()
		{
			Key = "Test",
			Interval = TimeSpan.Zero,
			InvokableType = null!,
			StartInterval = TimeSpan.Zero,
			RetryCount = 0,
			RetryInterval = TimeSpan.Zero,
		};

		this._eventRightEqual = new ScheduledEvent()
		{
			Key = "test",
			Interval = TimeSpan.Zero,
			InvokableType = null!,
			StartInterval = TimeSpan.Zero,
			RetryCount = 0,
			RetryInterval = TimeSpan.Zero,
		};

		this._eventLeftNotEqual = new ScheduledEvent()
		{
			Key = "Test 1",
			Interval = TimeSpan.Zero,
			InvokableType = null!,
			StartInterval = TimeSpan.Zero,
			RetryCount = 0,
			RetryInterval = TimeSpan.Zero,
		};

		this._eventRightNotEqual = new ScheduledEvent()
		{
			Key = "Test 2",
			Interval = TimeSpan.Zero,
			InvokableType = null!,
			StartInterval = TimeSpan.Zero,
			RetryCount = 0,
			RetryInterval = TimeSpan.Zero,
		};
	}

	[Fact]
    public void ScheduledEventWithSameCaseInsensitiveKeyShouldMatchEquality()
    {
		Assert.Equal(this._eventLeftEqual, this._eventRightEqual);
	}

	[Fact]
	public void ScheduledEventWithSameCaseInsensitiveKeyShouldMatchEqualityMethod()
	{
		Assert.True(this._eventLeftEqual.Equals(this._eventRightEqual));
	}

	[Fact]
	public void ScheduledEventWithSameCaseInsensitiveKeyShouldMatchEqualityMethodByObject()
	{
		Assert.True(this._eventLeftEqual.Equals((object)this._eventRightEqual));
	}

	[Fact]
	public void ScheduledEventWithDifferentKeyShouldNotMatchEquality()
	{
		Assert.NotEqual(this._eventLeftNotEqual, this._eventRightNotEqual);
	}

	[Fact]
	public void ScheduledEventWithSameCaseInsensitiveKeyShouldMatchOperator()
	{
		Assert.True(this._eventLeftEqual == this._eventRightEqual);
	}

	[Fact]
	public void ScheduledEventWithDifferentKeyShouldNotMatchOperator()
	{
		Assert.True(this._eventLeftNotEqual != this._eventRightNotEqual);
	}

	[Fact]
	public void ScheduledEventWithSameCaseInsensitiveKeyShouldMatchByHash()
	{
		Assert.Equal(this._eventLeftEqual.GetHashCode(), this._eventRightEqual.GetHashCode());
	}

	[Fact]
	public void ScheduledEventWithDifferentKeyShouldNotMatchByHash()
	{
		Assert.NotEqual(this._eventLeftNotEqual.GetHashCode(), this._eventRightNotEqual.GetHashCode());
	}
}