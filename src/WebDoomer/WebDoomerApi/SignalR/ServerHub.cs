using Microsoft.AspNetCore.SignalR;

namespace WebDoomerApi.SignalR;

internal sealed class ServerHub : Hub
{
	public const string ServerHubUrl = "api/ServerHub";
	public const string OnServerRefreshSignalKey = "OnServerRefresh";
}