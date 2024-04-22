using Microsoft.Extensions.Configuration;

namespace WebDoomerApi.Services;

public static class IServiceCollectionExtensions
{
	/// <summary>
	/// Adds the main server data provider to the service collection pipeline.
	/// </summary>
	/// <param name="serviceCollection">The service collection to add the provider into.</param>
	/// <param name="configuration">Configuration to configure the provider.</param>
	/// <param name="section">The section in the configuration where the configuration exists.</param>
	/// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
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
