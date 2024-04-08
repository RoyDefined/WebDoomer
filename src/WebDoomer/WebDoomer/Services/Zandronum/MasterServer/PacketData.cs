namespace WebDoomer.Zandronum;

internal static partial class PacketData
{
	/// <summary>
	/// The launcher master challenge data to pass when fetching the servers.
	/// </summary>
	public const int LauncherMasterChallenge = 5_660_028;

	/// <summary>
	/// The master server protocol version to pass when fetching the servers.
	/// </summary>
	public const short MasterServerVersion = 2;

	/// <summary>
	/// Constant used to indicate that the packet contains no more servers.
	/// </summary>
	public const int NoMoreServers = 0;
}
