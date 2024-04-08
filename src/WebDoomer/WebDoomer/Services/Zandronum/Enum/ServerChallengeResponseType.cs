namespace WebDoomer.Zandronum;

/// <summary>
/// The possible challenge response types that can occur when fetching servers from the master server.
/// </summary>
public enum ServerChallengeResponseType : byte
{
	/// <summary>
	/// Indicates no response.
	/// </summary>
	none,

	/// <summary>Indicates fetcher is ip banned.</summary>
	/// <remarks>Internal name is <c>MSC_IPISBANNED</c>.</remarks>
	ipIsBanned = 3,

	/// <summary>Indicates the request was ignored. IP has made a request in the past 3 seconds.</summary>
	/// <remarks>Internal name is <c>MSC_REQUESTIGNORED</c>.</remarks>
	requestIgnored,

	/// <summary>Indicates the request uses an older version of the master protocol.</summary>
	/// <remarks>Internal name is <c>MSC_WRONGVERSION</c>.</remarks>
	wrongVersion,

	/// <summary>Indicates the beginning of the server list.</summary>
	/// <remarks>Internal name is <c>MSC_BEGINSERVERLISTPART</c>.</remarks>
	beginServerListPart,
}
