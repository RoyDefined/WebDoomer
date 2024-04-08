namespace WebDoomer.Zandronum;

/// <summary>
/// The possible ending response types that exists at the end of each packet.
/// </summary>
internal enum ServerListEndResponseType : byte
{
	none,

	/// <summary>
	/// Indicates the end of the current list, and the end of all packets.
	/// </summary>
	/// <remarks>Internal name is <c>MSC_ENDSERVERLIST</c>.</remarks>
	endServerList = 2,

	/// <summary>
	/// Indicates the end of the current list, but more packets will be send.
	/// </summary>
	/// <remarks>Internal name is <c>MSC_ENDSERVERLISTPART</c>.</remarks>
	endServerListPart = 7,
}
