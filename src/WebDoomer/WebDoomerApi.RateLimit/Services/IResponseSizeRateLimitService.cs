using System.Net;

namespace WebDoomerApi.RateLimit;

public interface IResponseSizeRateLimitService
{
	void AddFetchedBytes(IPAddress ipAddress, long bytes);
	bool GetFetchedBytesLimitReached(IPAddress ipAddress);
}