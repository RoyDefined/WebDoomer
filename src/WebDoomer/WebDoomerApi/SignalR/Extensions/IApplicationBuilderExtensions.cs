namespace WebDoomerApi.SignalR;

public static class IApplicationBuilderExtensions
{
	public static IApplicationBuilder UseSignalRServerHub(this IApplicationBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		_ = app.UseRouting();
		return app.UseEndpoints(endpoints => endpoints.MapHub<ServerHub>("/" + ServerHub.ServerHubUrl));
	}
}
