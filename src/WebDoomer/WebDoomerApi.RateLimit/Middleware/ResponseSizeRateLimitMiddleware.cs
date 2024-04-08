using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebDoomerApi.RateLimit;

internal sealed class ResponseSizeRateLimitMiddleware
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="RequestDelegate"/>
	private readonly RequestDelegate _next;

	/// <inheritdoc cref="IResponseSizeRateLimitService"/>
	private readonly IResponseSizeRateLimitService _responseSizeRateLimitService;

	public ResponseSizeRateLimitMiddleware(
		ILogger<ResponseSizeRateLimitMiddleware> logger, 
		RequestDelegate next,
		IResponseSizeRateLimitService responseSizeRateLimitService)
	{
		this._logger = logger;
		this._next = next;
		this._responseSizeRateLimitService = responseSizeRateLimitService;
	}

	/// <inheritdoc />
	public async Task Invoke(HttpContext context)
	{
		var ip = context.Connection.RemoteIpAddress;
		var exceeded = this._responseSizeRateLimitService.GetFetchedBytesLimitReached(ip);

		if (exceeded)
		{
			this._logger.LogDebug("Rate limit triggered for user.");
			context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
			await context.Response.WriteAsync("Rate limit exceeded. Please try again later.")
				.ConfigureAwait(false);
			return;
		}

		// Replace response body to read the response ourselves without interrupting the existing stream.
		var originalResponseBody = context.Response.Body;
		using var responseBodyMemoryStream = new MemoryStream();
		context.Response.Body = responseBodyMemoryStream;

		await this._next(context)
			.ConfigureAwait(false);

		responseBodyMemoryStream.Position = 0;
		using var reader = new StreamReader(responseBodyMemoryStream);
		var content = await reader.ReadToEndAsync()
			.ConfigureAwait(false);

		this._responseSizeRateLimitService.AddFetchedBytes(ip, content.Length);

		responseBodyMemoryStream.Position = 0;
		await responseBodyMemoryStream.CopyToAsync(originalResponseBody)
			.ConfigureAwait(false);

		context.Response.Body = originalResponseBody;
	}
}
