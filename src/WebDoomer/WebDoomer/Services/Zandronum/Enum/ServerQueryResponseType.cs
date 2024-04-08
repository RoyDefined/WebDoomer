namespace WebDoomer.Zandronum;

/// <summary>
/// The possible ending response types that exists at the end of each packet when querying a server for its data.
/// </summary>
public enum ServerQueryResponseType : int
{
	/// <summary>
	/// Indicates no response.
	/// </summary>
	none,

	/// <remarks>Internal name is <c>SERVER_LAUNCHER_CHALLENGE</c>.</remarks>
	challenge = 5660023,

	/// <remarks>Internal name is <c>SERVER_LAUNCHER_IGNORING</c>.</remarks>
	ignoring = 5660024,

	/// <remarks>Internal name is <c>SERVER_LAUNCHER_BANNED</c>.</remarks>
	banned = 5660025,

	/// <remarks>Internal name is <c>SERVER_LAUNCHER_SEGMENTED_CHALLENGE</c>.</remarks>
	segmentedChallenge = 5660031,
}
