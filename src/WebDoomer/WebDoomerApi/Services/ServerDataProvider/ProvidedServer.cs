using Sqids;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using WebDoomer.Zandronum;

#pragma warning disable CA1056 // URI-like properties should not be strings
#pragma warning disable CA1054 // URI-like parameters should not be strings

namespace WebDoomerApi.Services;

/// <summary>
/// Represents a server provided by the provider to send over the network.
/// </summary>
public sealed record ProvidedServer(
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
	string? GameModeName,
	string? GameModeShortName,
	VoiceChatType? VoiceChatType)
{
	internal static ProvidedServer Create(ServerResult result, EngineType engine, SqidsEncoder<uint> encoder)
	{
		var addressBytes = result.EndPoint.Address.GetAddressBytes();
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(addressBytes);
		}

		var addressInt = BitConverter.ToUInt32(addressBytes, 0);
		var id = encoder.Encode(addressInt, (uint)result.EndPoint.Port);

		var playingClientCount = result.PlayerDataCollection?.Count(x => !x.IsBot && !x.IsSpectating);
		var spectatingClientCount = result.PlayerDataCollection?.Count(x => !x.IsBot && x.IsSpectating);
		var botCount = result.PlayerDataCollection?.Count(x => x.IsBot);

		return new(
			id,
			engine,
			playingClientCount,
			spectatingClientCount,
			botCount,
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
			result.PlayerCount,
			result.PlayerDataCollection,
			result.TeamCount,
			result.TeamInfoCollection,
			result.TestingServerName,
			result.DmFlagCollection,
			result.SecuritySettings,
			result.DehackedNameCollection,
			result.Country,
			result.GameModeName,
			result.GameModeShortName,
			result.VoiceChatType);
	}
}
