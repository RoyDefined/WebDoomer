using Microsoft.Extensions.DependencyInjection;
using WebDoomer.QZandronum;
using WebDoomer.Zandronum;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace WebDoomer;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddWebDoomer(
        this IServiceCollection serviceCollection,
		IConfiguration configuration,
		string section)
    {
		var configurationSection = configuration.GetSection(section);
		if (!configurationSection.Exists())
		{
			throw new ArgumentException("Configuration not found under the given section.", nameof(section));
		}

		// Register Zandronum servers
		_ = serviceCollection.AddSingleton<IZandronumMasterServerService, ZandronumMasterServerService>();
		_ = serviceCollection.AddSingleton<IZandronumServerService, ZandronumServerService>();
		_ = serviceCollection.Configure<ServerFetchOptions>(configurationSection);

		// Register QZandronum servers
		_ = serviceCollection.AddSingleton<IQZandronumMasterServerService, QZandronumMasterServerService>();
		_ = serviceCollection.AddSingleton<IQZandronumServerService, QZandronumServerService>();

		return serviceCollection;
    }
}