using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using WebDoomer.Packets;

namespace WebDoomer.Zandronum;

internal class ZandronumMasterServerService : IZandronumMasterServerService, IDisposable
{
	/// <inheritdoc cref="ILogger"/>
	protected readonly ILogger _logger;

	/// <summary>
	/// The service's options.
	/// </summary>
	private WebDoomerOptions _options;

	/// <summary>
	/// The service's option listener.
	/// </summary>
	private readonly IDisposable? _optionsMonitorListener;

	public ZandronumMasterServerService(
		ILogger<ZandronumMasterServerService> logger,
		IOptionsMonitor<WebDoomerOptions> optionsMonitor)
	{
		this._logger = logger;
		this._options = optionsMonitor.CurrentValue;
		this._optionsMonitorListener = optionsMonitor.OnChange(this.OptionsMonitorOnChangeListener);
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
		var timeoutTask = Task.Delay(this._options.MasterServer.FetchTaskTimeout, CancellationToken.None);

		var builder = new MasterServerResultBuilder();
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var bufferData = new byte[this._options.MasterServer.MaximumPacketSize];
			var socketResultTask = socket.ReceiveFromAsync(bufferData, endPoint, cancellationToken).AsTask();

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

	private void OptionsMonitorOnChangeListener(WebDoomerOptions options, string? _)
	{
		this._logger.LogDebug("Settings update observed.");
		this._options = options;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this._optionsMonitorListener?.Dispose();
	}
}
