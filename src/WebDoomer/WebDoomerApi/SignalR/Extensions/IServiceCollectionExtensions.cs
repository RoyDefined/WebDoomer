namespace WebDoomerApi.SignalR;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddSignalRServerHub(
		this IServiceCollection serviceCollection)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		_ = serviceCollection.AddSignalR();
		return serviceCollection;
	}
}
