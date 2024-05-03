using Microsoft.AspNetCore.Mvc;
using WebDoomerApi.Services;

namespace WebDoomerApi.Controllers;

/// <summary>
/// Represents the main server controller to handle requests related to servers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ServerController : ControllerBase
{
	/// <inheritdoc cref="ILogger"/>
	private readonly ILogger _logger;

	/// <inheritdoc cref="IServerDataProvider"/>
	private readonly IServerDataProvider _serverDataProvider;

	public ServerController(
		ILogger<ServerController> logger,
		IServerDataProvider serverDataProvider)
	{
		this._logger = logger;
		this._serverDataProvider = serverDataProvider;
	}

	/// <summary>
	/// Handles fetching servers by a optionally given order. Returns a list of strings that represents the servers.
	/// </summary>
	/// <param name="orderBy">Specifies an order on which to order the servers by.</param>
	/// <param name="search">An optional search string that specifies what a server name must contain to be returned.</param>
	/// <returns>A collection of <see cref="string"/> that represents the ids of the servers.</returns>
	[HttpGet("ids")]
	public IActionResult GetServerIds(OrderByType orderBy, string? search = null)
	{
		var result = this._serverDataProvider.GetServerIds(orderBy, search);
		return base.Ok(result);
	}

	/// <summary>
	/// Handles fetching servers by a optionally given range and order. Returns a list of servers with basic information.
	/// </summary>
	/// <param name="orderBy">Specifies an order on which to order the servers by.</param>
	/// <param name="skip">Specifies how many servers should be skipped before taking servers.</param>
	/// <param name="take">Specifies how many servers should be taken in the response.</param>
	/// <param name="search">An optional search string that specifies what a server name must contain to be returned.</param>
	/// <returns>A collection of <see cref="ListedServer"/>.</returns>
	[HttpGet("range")]
	public IActionResult GetServersRange(OrderByType orderBy, int skip = 0, int take = int.MaxValue, string? search = null)
	{
		var result = this._serverDataProvider.GetServersRange(orderBy, skip, take, search);
		var listedServers = result.Select(ListedServer.Create);
		return base.Ok(listedServers);
	}

	/// <summary>
	/// Handles fetching a server under a specified <paramref name="id"/> and returns detailed information.
	/// </summary>
	/// <param name="id">The id of the server to fetch.</param>
	/// <returns>A <see cref="ProvidedServer"/>. Else a <see cref="NotFoundResult"/> if the server was not found.</returns>
	[HttpGet("id/{id}")]
	public IActionResult GetServerById(string id)
	{
		var result = this._serverDataProvider.GetServerByIdOrDefault(id);
		if (result == null)
		{
			return base.NotFound();
		}

		return base.Ok(DetailedServer.Create(result));
	}
}
