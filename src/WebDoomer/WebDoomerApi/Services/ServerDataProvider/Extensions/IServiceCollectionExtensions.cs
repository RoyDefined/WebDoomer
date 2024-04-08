namespace WebDoomerApi.Services;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddServerDataProvider(
		this IServiceCollection serviceCollection)
	{
		_ = serviceCollection.AddSingleton<IServerDataProvider, ConcurrentServerDataProvider>();
		return serviceCollection;
	}
}
