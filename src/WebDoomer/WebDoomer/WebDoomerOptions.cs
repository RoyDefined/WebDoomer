using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDoomer;

/// <summary>
/// Represents the options used by WebDoomer.
/// </summary>
internal sealed class WebDoomerOptions
{
	internal sealed class MasterServerOptions
	{
		/// <summary>
		/// The time in milliseconds when a task to fetch all server IP endpoints should finish.
		/// </summary>
		public required int FetchTaskTimeout { get; init; }

		/// <summary>
		/// The maximum packet size that is allowed to be received by the master server.
		/// </summary>
		public required int MaximumPacketSize { get; init; }
	}

	internal sealed class ServerOptions
	{
		/// <summary>
		/// The number of end points that a single buffer and socket should handle.
		/// </summary>
		public required int EndPointsPerBuffer { get; init; }

		/// <summary>
		/// The socket buffer size that should be used for sending packages.
		/// </summary>
		public required int SocketSendBufferSize { get; init; }

		/// <summary>
		/// The socket buffer size that should be used for receiving packages.
		/// </summary>
		public required int SocketReceiveBufferSize { get; init; }

		/// <summary>
		/// The delay to apply between send packages to servers.
		/// </summary>
		public required int SendDelay { get; init; }

		/// <summary>
		/// The time in milliseconds when a task to fetch all server information should finish.
		/// </summary>
		public required int FetchTaskTimeout { get; init; }

		/// <summary>
		/// The maximum packet size that is allowed to be received by a server.
		/// </summary>
		public required int MaximumPacketSize { get; init; }
	}

	/// <summary>
	/// Represents options related to the master server and its fetch methods.
	/// </summary>
	public required MasterServerOptions MasterServer { get; init; }

	/// <summary>
	/// Represents options related to the servers and their fetch methods.
	/// </summary>
	public required ServerOptions Server { get; init; }
}
