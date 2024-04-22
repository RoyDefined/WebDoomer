namespace WebDoomerApi;

/// <summary>
/// Represents the options used by the API.
/// </summary>
internal sealed class ApiOptions
{
	/// <summary>
	/// Indicates the minimum percentage of servers that must be fetched before the pending list of switched with the actual provided server list.
	/// </summary>
	public required int MinimumPendingServerPercentage { get; init; }
}
