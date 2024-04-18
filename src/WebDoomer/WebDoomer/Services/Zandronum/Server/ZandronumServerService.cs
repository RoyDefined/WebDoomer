using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using WebDoomer.Packets;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace WebDoomer.Zandronum;

internal class ZandronumServerService : IZandronumServerService, IDisposable
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

	public ZandronumServerService(
		ILogger<ZandronumServerService> logger,
		IOptionsMonitor<WebDoomerOptions> optionsMonitor)
	{
		this._logger = logger;
		this._options = optionsMonitor.CurrentValue;
		this._optionsMonitorListener = optionsMonitor.OnChange(this.OptionsMonitorOnChangeListener);
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
		var resultEnumerable = this.GetServersDataAsync([endPoint], protocolType, flagset0, flagset1, cancellationToken)
			.ConfigureAwait(false);
		var resultArray = new ServerResult[1];
		await foreach(var result in resultEnumerable)
		{
			if (resultArray[0] != null)
			{
				Debug.Fail("Retrieved multiple server results. Only one was expected.");
				continue;
			}

			resultArray[0] = result;
		}

		if (resultArray[0] == null)
		{
			Debug.Fail("Expected a server result.");
		}

		return resultArray.Single();
	}

	/// <inheritdoc />
	public async virtual IAsyncEnumerable<ServerResult> GetServersDataAsync(IPEndPoint[] endPoints, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		// Divide endpoints over a number of sockets.
		var buffers = new List<IPEndPoint[]>();

		for (var i = 0; i < endPoints.Length; i += this._options.Server.EndPointsPerBuffer)
		{
			buffers.Add(endPoints.Skip(i).Take(this._options.Server.EndPointsPerBuffer).ToArray());
		}

		this._logger.LogInformation("Start fetching server data. Total sockets: {SocketCount}. Flag set 0: ({Flagset0Int}){Flagset0}, flag set 1: ({Flagset1Int}){Flagset1}.", buffers.Count, (uint)flagset0, flagset0, (uint)flagset1, flagset1);
		this._logger.LogDebug("Socket send buffer size: {SocketSendBufferSize}. Socket receive buffer size: {SocketReceiveBufferSize} Endpoints per buffer: {EndPointsPerBuffer}.", this._options.Server.SocketSendBufferSize, this._options.Server.SocketReceiveBufferSize, this._options.Server.EndPointsPerBuffer);

		// The packet to send to all servers.
		var packet = new HuffmanPacket(sizeof(int) * 4 + sizeof(byte))
			.Write(protocolType, flagset0, flagset1);

		var stopwatch = Stopwatch.StartNew();
		var bag = new ConcurrentBag<ServerResult>();
		var parallelTask = Parallel.ForEachAsync(buffers, cancellationToken, async (buffer, _) =>
		{
			using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SendBufferSize = this._options.Server.SocketSendBufferSize;
			socket.ReceiveBufferSize = this._options.Server.SocketReceiveBufferSize;

			var resultEnumerable = this.GetServersDataAsync(buffer, packet, socket, stopwatch, cancellationToken);
			await foreach(var result in resultEnumerable)
			{
				bag.Add(result);
			}
		});

		while(!parallelTask.IsCompleted || !bag.IsEmpty)
		{
			if (!bag.TryTake(out var result))
			{
				await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken)
					.ConfigureAwait(false);
				continue;
			}

			yield return result;
		}

		await parallelTask.ConfigureAwait(false);
		this._logger.LogInformation("Full fetch finished after {StopwatchMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
	}

	private async IAsyncEnumerable<ServerResult> GetServersDataAsync(IPEndPoint[] endPoints, Packet sendPacket, Socket socket, Stopwatch stopwatch, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		foreach (var endPoint in endPoints)
		{
			socket.SendTo(sendPacket, endPoint);

			await Task.Delay(this._options.Server.SendDelay, cancellationToken)
				.ConfigureAwait(false);
		}

		// This is the main dictionary that holds the builders to eventually return the results from.
		// Every time an endpoint is parsed and ready to build, the dictionary will remove an instance.
		var pendingBuilders = new Dictionary<IPEndPoint, ServerResultBuilder>(endPoints.Select(x => new KeyValuePair<IPEndPoint, ServerResultBuilder>(x, new(x))));

		// Main timeout indicates up to how long this task can run.
		var timeoutTask = Task.Delay(this._options.Server.FetchTaskTimeout, CancellationToken.None);

		while (pendingBuilders.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var bufferData = new byte[this._options.Server.MaximumPacketSize];

			// Listen for any endpoint rather than a specific one.
			var endPointAny = new IPEndPoint(IPAddress.Any, 0);

			// Base timeout that will end the method should servers not respond in time.
			var socketResultTask = socket.ReceiveFromAsync(bufferData, endPointAny, cancellationToken).AsTask();

			// Check if request timed out.
			var resultTask = await Task.WhenAny(socketResultTask, timeoutTask)
				.ConfigureAwait(false);
			if (resultTask == timeoutTask)
			{
				break;
			}
			SocketReceiveFromResult socketResult;
			try
			{
				socketResult = await socketResultTask
					.ConfigureAwait(false);
			}
			catch (SocketException ex)
			{
				this._logger.LogWarning(ex, "Failed to read bytes from socket. Response packet was likely too large.");
				this._logger.LogWarning("SocketException: {Message}", ex.Message);
				continue;
			}
			
			var data = bufferData.Take(socketResult.ReceivedBytes).ToArray();
			var remoteEndpoint = (IPEndPoint)socketResult.RemoteEndPoint;
			var receivePacket = new HuffmanPacket(data);

			this._logger.LogDebug("Received data from {EndPoint}. Size: {RegularSize}. Encoded size: {EncodedSize}", remoteEndpoint, receivePacket.PacketSize, receivePacket.EncodedPacketSize);

			// Get pending builder.
			// This should only ever error if data was returned from an unexpected endpoint.
			if (!pendingBuilders.TryGetValue(remoteEndpoint, out var builder))
			{
				this._logger.LogWarning("Could not find builder for {EndPoint}.", remoteEndpoint);
				continue;
			}

			ServerResult serverResult;
			try
			{
				// Continue fetching for the endpoint if more data is expected.
				if (builder.Parse(receivePacket))
				{
					if (receivePacket.UnreadBytes > 0)
					{
						this._logger.LogWarning("Continued packet of {EndPoint} contains unreadable bytes.", remoteEndpoint);
					}

					continue;
				}

				if (receivePacket.UnreadBytes > 0)
				{
					this._logger.LogWarning("Final packet of {EndPoint} contains unreadable bytes.", remoteEndpoint);
				}

				serverResult = builder.Build();
				_ = pendingBuilders.Remove(remoteEndpoint);
			}
			catch (Exception ex)
			{
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}

				this._logger.LogWarning("Failed to server data from {EndPoint}: {Exception}", remoteEndpoint, ex.Message);
				serverResult = builder.Build(ServerResultState.Error);
			}

			yield return serverResult;
		}

		// Handle unfinished builders.
		if (pendingBuilders.Count > 0)
		{
			foreach (var builder in pendingBuilders.Values)
			{
				yield return builder.Build(ServerResultState.TimeOut);
			}

			//this._logger.LogWarning("Unfinished or timed out endpoints: {EndPointsJoined}", pendingBuilders.Select(x => x.Value.endPoint));
		}

		this._logger.LogDebug("Batch fetch finished after {StopwatchMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
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
