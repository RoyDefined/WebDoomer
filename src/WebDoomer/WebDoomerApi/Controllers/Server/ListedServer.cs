using WebDoomerApi.Services;

namespace WebDoomerApi.Controllers;

/// <summary>
/// Represents a server variant that contains information suitable for a list.
/// </summary>
public sealed record ListedServer(
	string Id,
	EngineType Engine,
	string? Name,
	int? PlayingClientCount,
	int? SpectatingClientCount,
	int? BotCount,
	byte? MaxClients,
	string? Country)
{
	internal static ListedServer Create(ProvidedServer server)
	{
		return new(
			server.Id,
			server.Engine,
			server.Name,
			server.PlayingClientCount,
			server.SpectatingClientCount,
			server.BotCount,
			server.MaxClients,
			server.Country);
	}
}
