using System.Net;

namespace WebDoomer.Zandronum;
public interface IZandronumMasterServerService
{
	Task<MasterServerResult> GetMasterServerHostsAsync(IPAddress ipAddress, int port, CancellationToken cancellationToken = default);
	Task<MasterServerResult> GetMasterServerHostsAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default);
	Task<MasterServerResult> GetMasterServerHostsAsync(string hostAddress, int port, CancellationToken cancellationToken = default);
}