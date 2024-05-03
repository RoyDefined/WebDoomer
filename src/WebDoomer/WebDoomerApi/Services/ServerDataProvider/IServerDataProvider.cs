﻿using System.Collections.Concurrent;
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
	/// <param name="search">An optional search string to search for specific server names.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ProvidedServer"/>.</returns>
	IEnumerable<ProvidedServer> GetServersRange(OrderByType orderBy, int skip, int take, string? search);

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
	/// <param name="search">An optional search string to search for specific server names.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/> representing the ids of the servers.</returns>
	IEnumerable<string> GetServerIds(OrderByType orderBy, string? search);

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