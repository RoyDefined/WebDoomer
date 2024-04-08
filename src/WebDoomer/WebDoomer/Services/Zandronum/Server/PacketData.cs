namespace WebDoomer.Zandronum;

internal static partial class PacketData
{
	/// <summary>
	/// The old server protocol launcher version to pass when fetching the servers using the old protocol.
	/// </summary>
	public const int ServerQueryOldLauncherChallenge = 199;

	/// <summary>
	/// The new server protocol launcher version to pass when fetching the servers using the new protocol.
	/// </summary>
	public const int ServerQueryNewLauncherChallenge = 200;
}