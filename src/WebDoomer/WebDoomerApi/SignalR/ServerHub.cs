using Microsoft.AspNetCore.SignalR;

namespace WebDoomerApi.SignalR;

internal sealed class ServerHub : Hub
{
	public const string ServerHubUrl = "ServerHub";
	public const string OnServerRefreshSignalKey = "OnServerRefresh";
}