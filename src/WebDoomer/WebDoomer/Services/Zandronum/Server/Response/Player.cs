namespace WebDoomer.Zandronum;

/// <summary>
/// Represents a player in a server.
/// </summary>
public sealed class Player
{
	/// <summary>Player's name.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_NAME</c>.</remarks>
	public required string Name { get; init; }

	/// <summary>Player's <c>pointcount</c>/<c>fragcount</c>/<c>killcount</c>.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_FRAGPOINTKILLCOUNT</c>.</remarks>
	public required short ScoreCount { get; init; }

	/// <summary>Player's ping. Will be <see langword="null"/> if invalid or 0.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_NAME</c>.</remarks>
	public required short? Ping { get; init; }

	/// <summary>If <see langword="true"/>, the player is spectating.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_SPECTATING</c>.</remarks>
	public required bool IsSpectating { get; init; }

	/// <summary>If <see langword="true"/>, the player is a bot.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_BOT</c>.</remarks>
	public required bool IsBot { get; init; }

	/// <summary>The player's team. Will be <c>255</c> if the player has not joined a team. Will be <see langword="null"/> when the game mode is not a team game.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_TEAM</c>.</remarks>
	public required byte? Team { get; init; }

	/// <summary>The player's time in the server, in minutes.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA_PLAYTIME</c>.</remarks>
	public required byte PlayTime { get; init; }
}
