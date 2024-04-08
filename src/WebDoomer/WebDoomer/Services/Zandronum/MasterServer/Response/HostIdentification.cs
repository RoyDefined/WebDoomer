using System.Net;

namespace WebDoomer.Zandronum;

/// <summary>
/// Represents the identification of a host. Indicates host ip and the available ports serving a server.
/// </summary>
public sealed class HostIdentification
{
	internal HostIdentification(
		IPAddress address,
		List<ushort> ports)
	{
		this.Address = address;
		this.Ports = ports;
	}

	public IPAddress Address { get; }
    public List<ushort> Ports { get; }
}
