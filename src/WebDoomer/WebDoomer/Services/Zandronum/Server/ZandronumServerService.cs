using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebDoomer.Packets;

namespace WebDoomer.Zandronum;

internal class ZandronumServerService : IZandronumServerService
{
	/// <inheritdoc cref="ILogger"/>
	protected readonly ILogger _logger;
	public ZandronumServerService(
		ILogger<ZandronumServerService> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public async Task<ServerResult> GetServerDataAsync(IPAddress address, int port, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1, CancellationToken cancellationToken)
	{
		return await this.GetServerDataAsync(new(address, port), protocolType, flagset0, flagset1, cancellationToken)
			.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async virtual Task<ServerResult> GetServerDataAsync(IPEndPoint endPoint, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Fetching data from server {EndPoint}. Flag set 0: ({Flagset0Int}){Flagset0}, flag set 1: ({Flagset1Int}){Flagset1}.", endPoint, (uint)flagset0, flagset0, (uint)flagset1, flagset1);

		using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		var packet = new HuffmanPacket()
			.Write(protocolType, flagset0, flagset1);

		socket.SendTo(packet, endPoint);

		var builder = new ServerResultBuilder(endPoint);
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// TODO: set max size.
			var bufferData = new byte[9999];
			this._logger.LogDebug("Retrieving from socket.");

			// Retrieve with timeout to avoid waiting forever for a response.
			var socketResultTask = socket.ReceiveFromAsync(bufferData, endPoint, cancellationToken).AsTask();
			var timeoutTask = Task.Delay(8000, CancellationToken.None);

			// Check if request timed out.
			var resultTask = await Task.WhenAny(socketResultTask, timeoutTask)
				.ConfigureAwait(false);
			if (resultTask == timeoutTask)
			{
				return builder.Build(ServerResultState.TimeOut);
			}

			var socketResult = await socketResultTask
				.ConfigureAwait(false);
			var data = bufferData.Take(socketResult.ReceivedBytes).ToArray();
			var receivePacket = new HuffmanPacket(data);

			this._logger.LogDebug("Received packet. Size: {RegularSize}. Encoded size: {EncodedSize}", receivePacket.PacketSize, receivePacket.EncodedPacketSize);

			try
			{
				// Continue fetch if this is not the last packet.
				if (builder.Parse(receivePacket))
				{
					continue;
				}
			}
			catch (Exception ex)
			{
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}

				this._logger.LogWarning("Failed to server data from {EndPoint}: {Exception}", endPoint, ex.Message);
				return builder.Build(ServerResultState.Error);
			}

			return builder.Build();
		}
	}
}
