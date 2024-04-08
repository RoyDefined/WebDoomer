using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebDoomerApi.RateLimit;

public static class IApplicationBuilderExtensions
{
	public static IApplicationBuilder UseResponseSizeRateLimiting(this IApplicationBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		return app.UseMiddleware<ResponseSizeRateLimitMiddleware>();
	}
}
