using WebDoomer.Packets;

namespace WebDoomer.Zandronum;

internal static class PacketExtension
{
	/// <summary>
	/// Prepares the packet by writing the given protocol and flag sets into the packet.
	/// </summary>
	/// <param name="packet">The packet to write into.</param>
	/// <param name="protocolType">The protocol that should be written into the packet.</param>
	/// <param name="flagset0">Flag set 0 that will be written into the packet.</param>
	/// <param name="flagset1">Flag set 1 that will be written into the packet</param>
	/// <returns>The updated packet.</returns>
	public static Packet Write(this Packet packet, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1)
	{
		if (protocolType == LauncherProtocolType.NewProtocol)
		{
			_ = packet
				.Write(PacketData.ServerQueryNewLauncherChallenge)
				.Write((uint)flagset0)

				// Value is written later to properly determine ping.
				.Write(0)

				.Write((uint)flagset1);
		}
		else
		{
			_ = packet
				.Write(PacketData.ServerQueryOldLauncherChallenge)
				.Write((uint)flagset0)

				// Value is written later to properly determine ping.
				.Write(0)

				.Write((uint)flagset1);

			if (protocolType == LauncherProtocolType.OldProtocolSegmented)
			{
				_ = packet
					.Write((byte)1);
			}
		}
		return packet;
	}
}
