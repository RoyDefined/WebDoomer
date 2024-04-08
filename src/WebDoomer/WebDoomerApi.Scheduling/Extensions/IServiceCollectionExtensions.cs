using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace WebDoomerApi.Scheduling;

public static class IServiceCollectionExtensions
{
	/// <summary>
	/// Registers a scheduler in the application's service collection to handle scheduled tasks.
	/// </summary>
	/// <param name="serviceCollection">The service collection to inject the services into.</param>
	/// <returns>The updated service collection.</returns>
	public static IServiceCollection AddScheduling(
		this IServiceCollection serviceCollection)
	{
		_ = serviceCollection.AddHostedService<SchedulerHostedService>();
		_ = serviceCollection.AddSingleton<IScheduler, Scheduler>();

		// Time provider used by schedulers to determine next invokation.
		serviceCollection.TryAddSingleton(TimeProvider.System);

		// Main worker pooling.
		_ = serviceCollection.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
		_ = serviceCollection.AddSingleton(serviceProvider =>
		{
			var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
			var policy = new DefaultPooledObjectPolicy<SchedulerWorker>();
			return provider.Create(policy);
		});

		return serviceCollection;
	}
}
