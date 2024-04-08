using System.Net;
using WebDoomer.Zandronum;
using WebDoomerApi.Controllers;

namespace WebDoomerApi.Services;

/// <summary>
/// Represents the main provider of all the data related to servers.
/// </summary>
public interface IServerDataProvider
{
	/// <summary>
	/// Returns a range servers as ordered by <paramref name="orderBy"/>.
	/// </summary>
	/// <param name="orderBy">The type of ordering to apply to the servers.</param>
	/// <param name="skip">The number of items to skip.</param>
	/// <param name="take">The number of items to take.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ServerlistedRecord"/>.</returns>
	IEnumerable<ProvidedServer> GetServersRange(OrderByType orderBy, int skip, int take);

	/// <summary>
	/// Returns a server under the given <paramref name="id"/>.
	/// </summary>
	/// <param name="id">The id to search for.</param>
	/// <returns>A <see cref="ProvidedServer"/> or <see langword="null"/> if the server was not found.</returns>
	ProvidedServer? GetServerByIdOrDefault(string id);

	/// <summary>
	/// Returns all server ids as ordered by <paramref name="orderBy"/>.
	/// </summary>
	/// <param name="orderBy">The type of ordering to apply to the servers.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/> representing the id of a server.</returns>
	IEnumerable<string> GetServerIds(OrderByType orderBy);

	internal void SetData(EngineType engineType, IDictionary<IPAddress, ServerResult[]> data);
}