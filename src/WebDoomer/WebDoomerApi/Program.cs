using Serilog;
using WebDoomerApi.Jobs;
using WebDoomerApi.Scheduling;
using WebDoomer;
using WebDoomerApi.Services;
using WebDoomerApi.RateLimit;
using Microsoft.AspNetCore.HttpOverrides;


#if DEBUG
using Serilog.Debugging;
#endif

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
// This allows Serilog to log any errors during initialization.
SelfLog.Enable(Console.Error.WriteLine);
#endif

var logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.CreateLogger();

// Static logger for developmental purposes or where DI is not accessible. Use the injected ILogger for anything else.
Log.Logger = logger;

logger.Debug("Configuring service collection...");

try
{
	// Provide Serilog as the main logger.
	_ = builder.Logging.ClearProviders();
	_ = builder.Services.AddLogging(builder => builder.AddSerilog(logger));

	_ = builder.Services.AddControllers();
	_ = builder.Services.AddHttpContextAccessor();

#if DEBUG
	_ = builder.Services.AddCors();
#endif

	// WebDoomer
	_ = builder.Services.AddWebDoomer(builder.Configuration, "WebDoomerOptions");

	// Rate limiting
	_ = builder.Services.AddResponseSizeRateLimiting();

	// Scheduler
	_ = builder.Services.AddSingleton<ServerDataFetchJob>();
	_ = builder.Services.AddServerDataProvider(builder.Configuration, "ApiOptions");
	_ = builder.Services.AddScheduling();
}
catch (Exception ex)
{
	logger.Error(ex, "Error during initial builder setup.");
	return;
}

var app = builder.Build();
logger.Debug("Starting application...");

try
{
#if DEBUG
	_ = app.UseCors(configure => configure.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
		.WithOrigins("http://localhost:4200"));
#endif

	_ = app.UseForwardedHeaders(new ForwardedHeadersOptions
	{
		ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
	});

	_ = app.UseResponseSizeRateLimiting();
	_ = app.UseAuthorization();

#if RELEASE
    app.UseDefaultFiles();
    app.UseStaticFiles();
#endif

	_ = app.MapControllers();

#if RELEASE
	// The server will fall back any request not pointing to `api/` to the index html file,
    // because Angular will attempt to fetch an url it can't handle itself.
    // If the request does point to `api/` then this must point to a valid endpoint,
    // otherwise a 404 is returned.
	_ = app.Map("api/{**slug}", (HttpContext context) =>
	{
		context.Response.StatusCode = StatusCodes.Status404NotFound;
		return Task.CompletedTask;
	});
	_ = app.MapFallbackToFile("index.html");
#endif

	// Scheduling
	ServerDataFetchJob.Register(app);
}
catch (Exception ex)
{
	logger.Error(ex, "Error during application startup.");
	return;
}

var appProcess = app.RunAsync(CancellationToken.None);
logger.Information("Application has started.");

try
{
	await appProcess;
}
catch (Exception ex)
{
	logger.Error(ex, "Error during application lifetime.");
	return;
}