using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using WebDoomer.Packets;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
		this._logger.LogDebug("Start fetching server data. Flag set 0: ({Flagset0Int}){Flagset0}, flag set 1: ({Flagset1Int}){Flagset1}.", (uint)flagset0, flagset0, (uint)flagset1, flagset1);

		using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		var packet = new HuffmanPacket()
			.Write(protocolType, flagset0, flagset1);

		foreach(var endPoint in endPoints)
		{
			socket.SendTo(packet, endPoint);
		}

		// This is the main dictionary that holds the builders to eventually return the results from.
		// Every time an endpoint is parsed and ready to build, the dictionary will remove an instance.
		var pendingBuilders = new Dictionary<IPEndPoint, ServerResultBuilder>(endPoints.Select(x => new KeyValuePair<IPEndPoint, ServerResultBuilder>(x, new(x))));
		
		// Main timeout indicates up to how long this task can run.
		// TODO: Configurable.
		var timeoutTask = Task.Delay(15000, CancellationToken.None);

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// TODO: set max size.
			var bufferData = new byte[9999];

			// Listen for any endpoint.
			var endPoint = new IPEndPoint(IPAddress.Any, 0);

			// Base timeout that will end the method should servers not respond in time.
			var socketResultTask = socket.ReceiveFromAsync(bufferData, endPoint, cancellationToken).AsTask();

			// Check if request timed out.
			var resultTask = await Task.WhenAny(socketResultTask, timeoutTask)
				.ConfigureAwait(false);
			if (resultTask == timeoutTask)
			{
				break;
			}

			var socketResult = await socketResultTask
				.ConfigureAwait(false);
			var data = bufferData.Take(socketResult.ReceivedBytes).ToArray();
			var remoteEndpoint = (IPEndPoint)socketResult.RemoteEndPoint;
			var receivePacket = new HuffmanPacket(data);

			this._logger.LogDebug("Received data from {EndPoint}. Size: {RegularSize}. Encoded size: {EncodedSize}", remoteEndpoint, receivePacket.PacketSize, receivePacket.EncodedPacketSize);
			
			// Get pending builder.
			// This should only ever error if data was returned from an unexpected endpoint.
			if (!pendingBuilders.TryGetValue(remoteEndpoint, out var builder))
			{
				this._logger.LogError("Could not find builder for {EndPoint}.", remoteEndpoint);
				continue;
			}

			ServerResult serverResult;
			try
			{
				// Continue fetching for the endpoint if more data is expected.
				if (builder.Parse(receivePacket))
				{
					continue;
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
	}
}
