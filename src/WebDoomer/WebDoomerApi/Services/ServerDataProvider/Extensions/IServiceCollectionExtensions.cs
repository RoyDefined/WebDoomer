using Microsoft.Extensions.Configuration;

namespace WebDoomerApi.Services;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddServerDataProvider(
		this IServiceCollection serviceCollection,
		IConfiguration configuration,
		string section)
	{
		ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
		ArgumentNullException.ThrowIfNull(section, nameof(section));

		var configurationSection = configuration.GetSection(section);
		if (!configurationSection.Exists())
		{
			throw new ArgumentException("Configuration not found under the given section.", nameof(section));
		}

		_ = serviceCollection.AddSingleton<IServerDataProvider, ConcurrentServerDataProvider>();
		_ = serviceCollection.Configure<ApiOptions>(configurationSection);
		return serviceCollection;
	}
}
