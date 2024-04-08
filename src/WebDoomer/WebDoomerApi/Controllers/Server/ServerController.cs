using Microsoft.AspNetCore.Mvc;
using WebDoomerApi.Services;

namespace WebDoomerApi.Controllers;

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

	[HttpGet("range")]
	public IActionResult GetServersRange(OrderByType orderBy, int skip = 0, int take = int.MaxValue)
	{
		var result = this._serverDataProvider.GetServersRange(orderBy, skip, take);
		var listedServers = result.Select(ListedServer.Create);
		return base.Ok(listedServers);
	}

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

	[HttpGet("ids")]
	public IActionResult GetServerIds(OrderByType orderBy)
	{
		var result = this._serverDataProvider.GetServerIds(orderBy);
		return base.Ok(result);
	}
}
