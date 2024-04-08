using Microsoft.Extensions.DependencyInjection;
using WebDoomer.QZandronum;
using WebDoomer.Zandronum;

namespace WebDoomer;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddWebDoomer(
        this IServiceCollection serviceCollection)
    {
		// Register Zandronum servers
		_ = serviceCollection.AddSingleton<IZandronumMasterServerService, ZandronumMasterServerService>();
		_ = serviceCollection.AddSingleton<IZandronumServerService, ZandronumServerService>();

		// Register QZandronum servers
		_ = serviceCollection.AddSingleton<IQZandronumMasterServerService, QZandronumMasterServerService>();
		_ = serviceCollection.AddSingleton<IQZandronumServerService, QZandronumServerService>();

		return serviceCollection;
    }
}