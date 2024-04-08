using System.Net;

namespace WebDoomer.Zandronum;
public interface IZandronumServerService
{
	Task<ServerResult> GetServerDataAsync(IPAddress address, int port, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1, CancellationToken cancellationToken = default);
	Task<ServerResult> GetServerDataAsync(IPEndPoint endPoint, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1, CancellationToken cancellationToken = default);
}