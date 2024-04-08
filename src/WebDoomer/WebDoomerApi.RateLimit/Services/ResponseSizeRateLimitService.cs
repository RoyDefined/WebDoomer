using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace WebDoomerApi.RateLimit;

internal sealed class ResponseSizeRateLimitService : IResponseSizeRateLimitService
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="TimeProvider"/>
	private readonly TimeProvider _timeProvider;

	/// <inheritdoc cref="IMemoryCache"/>
	private readonly IMemoryCache _memoryCache;

	/// <summary>
	/// The total timespan that indicates the period bytes are cached.
	/// </summary>
	private readonly TimeSpan _cacheTime;

	/// <summary>
	/// The total size of the cache in bytes.
	/// </summary>
	private readonly long _cacheSize;

	public ResponseSizeRateLimitService(
		ILogger<ResponseSizeRateLimitService> logger,
		TimeProvider timeProvider,
		IMemoryCache memoryCache)
	{
		this._logger = logger;
		this._timeProvider = timeProvider;
		this._memoryCache = memoryCache;

		// TODO: Make configurable.
		this._cacheTime = TimeSpan.FromMinutes(10);
		this._cacheSize = 5 * 1024 * 1024;
	}

	/// <inheritdoc />
	public void AddFetchedBytes(IPAddress ipAddress, long bytes)
	{
		this._logger.LogDebug("Add bytes fetched by {IPipAddress}: {Bytes}", ipAddress, bytes);

		var now = this._timeProvider.GetUtcNow();
		var expiration = now.Add(this._cacheTime);
		var ipAddressString = ipAddress.ToString();
		var key = $"{ipAddressString}_{now.Minute}";

		var value = this._memoryCache.Get<long>(key);
		_ = this._memoryCache.Set(key, value + bytes, expiration);
	}

	/// <inheritdoc />
	public bool GetFetchedBytesLimitReached(IPAddress ipAddress)
	{
		var result = this.EnumerateFetchedBytes(ipAddress).Sum();
		this._logger.LogInformation("Total bytes fetched by {IPipAddress}: {Bytes}", ipAddress, result);
		return result > this._cacheSize;
	}

	private IEnumerable<long> EnumerateFetchedBytes(IPAddress ipAddress)
	{
		var now = this._timeProvider.GetUtcNow();
		var time = now.Add(-this._cacheTime);
		var ipAddressString = ipAddress.ToString();

		for (; time <= now; time = time.AddMinutes(1))
		{
			var key = $"{ipAddressString}_{time.Minute}";
			if (this._memoryCache.TryGetValue(key, out long bytes))
			{
				yield return bytes;
			}
		}
	}
}
