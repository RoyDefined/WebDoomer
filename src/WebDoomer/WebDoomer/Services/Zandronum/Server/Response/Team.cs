namespace WebDoomer.Zandronum;

/// <summary>
/// Represents a team in a server.
/// </summary>
public sealed class Team
{
	/// <summary>Team's name.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_NAME</c>.</remarks>
	public required string? Name { get; init; }

	/// <summary>Team's color.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_COLOR</c>.</remarks>
	public required int? Color { get; init; }

	/// <summary>Team's score.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_SCORE</c>.</remarks>
	public required short? Score { get; init; }
}
