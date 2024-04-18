using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDoomer.Zandronum;

internal sealed class ServerFetchOptions
{
	public required int EndPointsPerBuffer { get; init; }
	public required int MaximumPacketSize { get; init; }
	public required int SocketSendBufferSize { get; init; }
	public required int SocketReceiveBufferSize { get; init; }
	public required int SendDelayMilliseconds { get; init; }
	public required int FetchTaskTimeoutMilliseconds { get; init; }
}
