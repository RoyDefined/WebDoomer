using Microsoft.AspNetCore.Hosting.Server;
using Sqids;
using System.Collections.ObjectModel;
using WebDoomer.Zandronum;

#pragma warning disable CA1056 // URI-like properties should not be strings
#pragma warning disable CA1054 // URI-like parameters should not be strings

namespace WebDoomerApi.Services;

/// <summary>
/// Represents a server variant that contains all information for a detailed server view.
/// </summary>
public sealed record DetailedServer(
	string Id,
	EngineType Engine,
	int? PlayingClientCount,
	int? SpectatingClientCount,
	int? BotCount,
	string? Name,
	string? Url,
	string? Email,
	string? MapName,
	byte? MaxClients,
	byte? MaxPlayers,
	ReadOnlyCollection<PWad>? PwadCollection,
	GameModeType? GameType,
	bool? GameTypeInstagib,
	bool? GameTypeBuckshot,
	GameName? GameNameType,
	string? Iwad,
	bool? ForcePassword,
	bool? ForceJoinPassword,
	GameSkill? GameSkillType,
	BotSkill? BotSkillType,
	short? FragLimit,
	short? TimeLimit,
	short? TimeLeft,
	short? DuelLimit,
	short? PointLimit,
	short? WinLimit,
	float? TeamDamage,
	byte? NumPlayers,
	ReadOnlyCollection<Player>? PlayerDataCollection,
	byte? TeamCount,
	ReadOnlyCollection<Team>? TeamInfoCollection,
	string? TestingServerName,
	ReadOnlyCollection<int>? DmFlagCollection,
	bool? SecuritySettings,
	ReadOnlyCollection<string>? DehackedNameCollection,
	string? Country,
	VoiceChatType? VoiceChatType,
	string? GameModeName,
	string? GameModeShortName)
{
	internal static DetailedServer Create(ProvidedServer result)
	{
		return new(
			result.Id,
			result.Engine,
			result.PlayingClientCount,
			result.SpectatingClientCount,
			result.BotCount,
			result.Name,
			result.Url,
			result.Email,
			result.MapName,
			result.MaxClients,
			result.MaxPlayers,
			result.PwadCollection,
			result.GameType,
			result.GameTypeInstagib,
			result.GameTypeBuckshot,
			result.GameNameType,
			result.Iwad,
			result.ForcePassword,
			result.ForceJoinPassword,
			result.GameSkillType,
			result.BotSkillType,
			result.FragLimit,
			result.TimeLimit,
			result.TimeLeft,
			result.DuelLimit,
			result.PointLimit,
			result.WinLimit,
			result.TeamDamage,
			result.NumPlayers,
			result.PlayerDataCollection,
			result.TeamCount,
			result.TeamInfoCollection,
			result.TestingServerName,
			result.DmFlagCollection,
			result.SecuritySettings,
			result.DehackedNameCollection,
			result.Country,
			result.VoiceChatType,
			result.GameModeName,
			result.GameModeShortName);
	}
}
