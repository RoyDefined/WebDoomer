namespace WebDoomer.Zandronum;

/// <summary>
/// Specifies the type of protocol to use when fetching data from a server.
/// </summary>
public enum LauncherProtocolType
{
	/// <summary>
	/// Use the old, unsegmented protocol.
	/// </summary>
	OldProtocol,

	/// <summary>
	/// Use the old protocol, but request a segmented response.
	/// </summary>
	OldProtocolSegmented,

	/// <summary>
	/// Use the new, segmented protocol. Note this protocol is only supported with servers running <c>3.2-alpha</c> and later.
	/// </summary>
	NewProtocol,
}
