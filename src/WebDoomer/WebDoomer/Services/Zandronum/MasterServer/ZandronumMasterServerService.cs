using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using WebDoomer.Packets;

namespace WebDoomer.Zandronum;

internal class ZandronumMasterServerService : IZandronumMasterServerService
{
	/// <inheritdoc cref="ILogger"/>
	protected readonly ILogger _logger;

	// TODO: Make these variables configurable.
	private readonly int _maximumPacketSize = 5000;
	private readonly TimeSpan _fetchTaskTimeout = TimeSpan.FromSeconds(15);

	public ZandronumMasterServerService(
		ILogger<ZandronumMasterServerService> logger)
	{
		this._logger = logger;
	}

	/// <inheritdoc />
	public async Task<MasterServerResult> GetMasterServerHostsAsync(string hostAddress, int port, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Resolving master server address {HostAddress}.", hostAddress);

		IPHostEntry entries;
		try
		{
			entries = await Dns.GetHostEntryAsync(hostAddress, CancellationToken.None)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to resolve host address {hostAddress}", ex);
		}

		if (entries.AddressList.Length == 0)
		{
			throw new InvalidOperationException($"Host address {hostAddress} resulted in no IP addresses.");
		}
		if (entries.AddressList.Length > 1)
		{
			this._logger.LogInformation("Host name {HostAddress} resulted in {Count} IP addresses. Using first.", hostAddress, entries.AddressList.Length);
		}

		return await this.GetMasterServerHostsAsync(entries.AddressList.First(), port, cancellationToken)
			.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<MasterServerResult> GetMasterServerHostsAsync(IPAddress ipAddress, int port, CancellationToken cancellationToken)
	{
		return await this.GetMasterServerHostsAsync(new IPEndPoint(ipAddress, port), cancellationToken)
			.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<MasterServerResult> GetMasterServerHostsAsync(IPEndPoint endPoint, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Fetching server hosts from master server {IPEndpoint}.", endPoint.ToString());

		using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		var packet = new HuffmanPacket(sizeof(int) + sizeof(short))
			.Write(PacketData.LauncherMasterChallenge)
			.Write(PacketData.MasterServerVersion);

		socket.SendTo(packet, endPoint);

		var builder = new MasterServerResultBuilder();
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var bufferData = new byte[this._maximumPacketSize];
			this._logger.LogDebug("Retrieving from socket.");

			// Retrieve with timeout to avoid waiting forever for a response.
			var socketResultTask = socket.ReceiveFromAsync(bufferData, endPoint, cancellationToken).AsTask();
			var timeoutTask = Task.Delay(this._fetchTaskTimeout, CancellationToken.None);

			// Check if request timed out.
			var resultTask = await Task.WhenAny(socketResultTask, timeoutTask)
				.ConfigureAwait(false);
			if (resultTask == timeoutTask)
			{
				return builder.Build(true);
			}

			var socketResult = await socketResultTask
				.ConfigureAwait(false);

			var data = bufferData.Take(socketResult.ReceivedBytes).ToArray();
			var receivePacket = new HuffmanPacket(data);

			this._logger.LogDebug("Received packet. Size: {RegularSize}. Encoded size: {EncodedSize}", receivePacket.PacketSize, receivePacket.EncodedPacketSize);

			// Continue fetch if this is not the last packet.
			if (builder.Parse(receivePacket))
			{
				continue;
			}

			return builder.Build(false);
		}
	}
}
