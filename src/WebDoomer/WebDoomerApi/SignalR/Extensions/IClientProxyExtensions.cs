using Microsoft.AspNetCore.SignalR;

namespace WebDoomerApi.SignalR;

public static class IClientProxyExtensions
{
	public static async Task<IClientProxy> OnServerRefreshAsync(this IClientProxy clientProxy, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(clientProxy);
		await clientProxy.SendAsync(ServerHub.OnServerRefreshSignalKey, cancellationToken);
		return clientProxy;
	}
}
