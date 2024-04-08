using System.Net;
using WebDoomer.Packets;

namespace WebDoomer.Zandronum;

internal sealed class MasterServerResultBuilder
{
	public MasterServerResultBuilder()
	{
		this.hosts = new();
	}

	internal ServerChallengeResponseType challengeResponse;
	internal readonly List<HostIdentification> hosts;

	/// <summary>
	/// Parses the packet into the builder.
	/// </summary>
	/// <param name="packet">The packet to parse.</param>
	/// <returns> Returns <see langword="true"/> if more packets are expected.</returns>
	public bool Parse(Packet packet)
	{
		try
		{
			this.challengeResponse = (ServerChallengeResponseType)packet.GetInt();

			// Packet was not a succesful response.
			if (this.challengeResponse != ServerChallengeResponseType.beginServerListPart)
			{
				return false;
			}

			// Packet number and MSC_SERVERBLOCK, both unused.
			_ = packet.GetByte();
			_ = packet.GetByte();

			var @continue = this.ParseHostIdentifications(packet);
			return @continue;
		}
		catch (Exception ex)
		{
			throw new ArgumentException($"Failed to parse packet into {nameof(MasterServerResultBuilder)}.", ex);
		}
	}

	private bool ParseHostIdentifications(Packet packet)
	{
		// Recursively parsing, the packet will indicate when it's finished.
		while (true)
		{
			// Should not be reached as the packet indicates when it's finished.
			if (packet.UnreadBytes <= 0)
			{
				throw new InvalidOperationException($"Packet was incorrectly finalized and is possibly corrupt. Packet no longer contains data even though it is expected to have bytes.");
			}

			// Server port count will be 0 if there are no more servers to be parsed.
			// At this point the packet will point out if there are still packets to come.
			var serverPortCount = packet.GetByte();
			if (serverPortCount == PacketData.NoMoreServers)
			{
				var endResponse = (ServerListEndResponseType)packet.GetByte();
				return endResponse == ServerListEndResponseType.endServerListPart;
			}

			var address = new IPAddress(new[] { packet.GetByte(), packet.GetByte(), packet.GetByte(), packet.GetByte() });
			var ports = Enumerable.Range(0, serverPortCount)
				.Select(x => packet.GetUShort())
				.ToList();
			var hostIdentification = new HostIdentification(address, ports);
			this.hosts.Add(hostIdentification);
		}
	}

	public MasterServerResult Build(bool timedOut)
	{
		return MasterServerResult.Create(this, timedOut);
	}
}
