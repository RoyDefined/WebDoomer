using System.Net;
using System.Net.Sockets;
using WebDoomer.Packets;

namespace WebDoomer.Zandronum;

internal static class SocketExtensions
{
	/// <summary>
	/// Sends the packet to the given <paramref name="endPoint"/>.
	/// </summary>
	/// <param name="socket">The currently opened socket connection.</param>
	/// <param name="packet">The packet to send over the socket.</param>
	/// <param name="endPoint">The target endpoint to send the packet to.</param>
	public static void SendTo(this Socket socket, Packet packet, IPEndPoint endPoint)
	{
		_ = socket.SendTo(packet.GetBuffer(), SocketFlags.None, endPoint);
	}
}
