using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebDoomerApi.RateLimit;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddResponseSizeRateLimiting(
		this IServiceCollection serviceCollection)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);

		serviceCollection.TryAddSingleton<IMemoryCache, MemoryCache>();
		_ = serviceCollection.AddSingleton<IResponseSizeRateLimitService, ResponseSizeRateLimitService>();
		return serviceCollection;
	}
}
