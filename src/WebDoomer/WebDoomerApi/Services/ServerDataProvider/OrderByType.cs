namespace WebDoomerApi.Services;

/// <summary>
/// Specifies the order operation on a listed result of servers.
/// </summary>
public enum OrderByType
{
	/// <summary>
	/// No order.
	/// </summary>
	None,

	/// <summary>
	/// Order by players in ascending order.
	/// </summary>
	PlayersAscending,

	/// <summary>
	/// Order by players in descending order.
	/// </summary>
	PlayersDescending,
}
