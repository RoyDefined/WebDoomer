using System.Collections.Concurrent;
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
	/// Returns a server under the given <paramref name="id"/>.
	/// </summary>
	/// <param name="id">The id to search for.</param>
	/// <returns>A <see cref="ProvidedServer"/> or <see langword="null"/> if the server was not found.</returns>
	ProvidedServer? GetServerByIdOrDefault(string id);

	/// <summary>
	/// Returns the server id found under the included <see cref="IPEndPoint"/>. If no server was found, returns <see langword="null"/>.
	/// </summary>
	/// <param name="search">Specifies a specific IP endpoint to search for.</param>
	/// <returns>An <see cref="string"/> representing the id of the server under the given endPoint. If no server was found, returns <see langword="null"/>.</returns>
	string? GetServerId(IPEndPoint search);

	/// <summary>
	/// Returns all server ids as ordered by <paramref name="orderBy"/>.
	/// </summary>
	/// <param name="search">An optional search string to search for specific server names.</param>
	/// <param name="orderBy">Optionally the type of ordering to apply to the servers.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/> representing the ids of the servers.</returns>
	IEnumerable<string> GetServerIds(string? search = null, OrderByType orderBy = OrderByType.PlayersDescending);

	/// <summary>
	/// Returns all server ids as ordered by <paramref name="orderBy"/>.
	/// </summary>
	/// <param name="search">Specifies a specific IP address to search for.</param>
	/// <param name="orderBy">Optionally the type of ordering to apply to the servers.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/> representing the ids of the servers.</returns>
	IEnumerable<string> GetServerIds(IPAddress search, OrderByType orderBy = OrderByType.PlayersDescending);

	/// <summary>
	/// Returns the server found under the included <see cref="IPEndPoint"/>. If no server was found, returns <see langword="null"/>.
	/// </summary>
	/// <param name="search">Specifies a specific IP endpoint to search for.</param>
	/// <returns>An <see cref="ProvidedServer"/ representing the id of the server under the given endPoint. If no server was found, returns <see langword="null"/>.</returns>
	ProvidedServer? GetServersRange(IPEndPoint search);

	/// <summary>
	/// Returns a range servers as ordered by <paramref name="orderBy"/>.
	/// </summary>
	/// <param name="search">An optional search string to search for specific server names.</param>
	/// <param name="skip">Optionally the number of items to skip.</param>
	/// <param name="take">Optionally the number of items to take.</param>
	/// <param name="orderBy">Optionally the type of ordering to apply to the servers.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ProvidedServer"/>.</returns>
	IEnumerable<ProvidedServer> GetServersRange(string? search = null, int skip = default, int take = default, OrderByType orderBy = OrderByType.PlayersDescending);

	/// <summary>
	/// Returns a range servers as ordered by <paramref name="orderBy"/>.
	/// </summary>
	/// <param name="skip">The number of items to skip.</param>
	/// <param name="take">The number of items to take.</param>
	/// <param name="search">Specifies a specific IP address to search for.</param>
	/// <param name="orderBy">Optionally the type of ordering to apply to the servers.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ProvidedServer"/>.</returns>
	IEnumerable<ProvidedServer> GetServersRange(IPAddress search, int skip = default, int take = default, OrderByType orderBy = OrderByType.PlayersDescending);

	/// <summary>
	/// Indicates the start of a new stream of data for the given <paramref name="engineType"/>
	/// </summary>
	/// <param name="engineType">The engine type that will have its data added.</param>
	/// <param name="expectedCount">Indicates the expected number of servers to arrive. Used to transfer pending data over to actual data.</param>
	internal void StartSetData(EngineType engineType, int expectedCount);

	/// <summary>
	/// Adds the given <paramref name="serverResult"/> to the collection of the given <paramref name="engineType"/>.
	/// </summary>
	/// <param name="engineType">The engine type under which to add the data.</param>
	/// <param name="serverResult">The result to add to the engine.</param>
	internal void AddData(EngineType engineType, ServerResult serverResult);

	/// <summary>
	/// Indicates the end of the stream of data for the given <paramref name="engineType"/> and the start of the finalization.
	/// </summary>
	/// <param name="engineType">The engine type that has all its data added.</param>
	internal void EndSetData(EngineType engineType);
}