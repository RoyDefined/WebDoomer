using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDoomer;

internal sealed class WebDoomerOptions
{
	internal sealed class MasterServerOptions
	{
		public required int FetchTaskTimeout { get; init; }
		public required int MaximumPacketSize { get; init; }
	}

	internal sealed class ServerOptions
	{
		public required int EndPointsPerBuffer { get; init; }
		public required int SocketSendBufferSize { get; init; }
		public required int SocketReceiveBufferSize { get; init; }
		public required int SendDelay { get; init; }
		public required int FetchTaskTimeout { get; init; }
		public required int MaximumPacketSize { get; init; }
	}

	public required MasterServerOptions MasterServer { get; init; }
	public required ServerOptions Server { get; init; }
}
