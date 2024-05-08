using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
	public IActionResult GetServerIds(OrderByType orderBy = OrderByType.PlayersDescending, string? search = null)
	{
		IEnumerable<string> results;
		if (this.TryGetIPEndPoint(search, out var ipEndPoint))
		{
			var result = this._serverDataProvider.GetServerId(ipEndPoint);
			if (result == null)
			{
				// TODO: Support NotFound.
				return base.Ok(Array.Empty<string>());
			}

			results = [result];
		}

		else
		{
			results = this.TryGetIPAddress(search, out var ipAddress)
				? this._serverDataProvider.GetServerIds(ipAddress, orderBy)
				: this._serverDataProvider.GetServerIds(search, orderBy);
		}

		return base.Ok(results);
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
	public IActionResult GetServersRange(OrderByType orderBy = OrderByType.PlayersDescending, int skip = 0, int take = int.MaxValue, string? search = null)
	{
		IEnumerable<ProvidedServer> results;
		if (this.TryGetIPEndPoint(search, out var ipEndPoint))
		{
			var result = this._serverDataProvider.GetServersRange(ipEndPoint);
			if (result == null)
			{
				// TODO: Support NotFound.
				return base.Ok(Array.Empty<ProvidedServer>());
			}

			results = [result];
		}

		else
		{
			results = this.TryGetIPAddress(search, out var ipAddress)
				? this._serverDataProvider.GetServersRange(ipAddress, skip, take, orderBy)
				: this._serverDataProvider.GetServersRange(search, skip, take, orderBy);
		}

		var listedServers = results.Select(ListedServer.Create);
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

	private bool TryGetIPEndPoint(string? search, [NotNullWhen(true)] out IPEndPoint? ipEndPoint)
	{
		if (search == null)
		{
			ipEndPoint = null;
			return false;
		}

		// It is possible the user supplied `zan://` with their search string when they copied the join button address.
		if (search.StartsWith("zan://"))
		{
			search = search["zan://".Length..];
		}

		var parsed = IPEndPoint.TryParse(search, out ipEndPoint);
		if (!parsed)
		{
			return false;
		}

		return ipEndPoint!.Port != default;
	}

	private bool TryGetIPAddress(string? search, [NotNullWhen(true)] out IPAddress? ipAddress)
	{
		if (search == null)
		{
			ipAddress = null;
			return false;
		}

		return IPAddress.TryParse(search, out ipAddress);
	}
}
