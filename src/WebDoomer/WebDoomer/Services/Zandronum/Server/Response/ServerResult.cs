using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace WebDoomer.Zandronum;

#pragma warning disable CA1056 // URI-like properties should not be strings

/// <summary>
/// Represents the result of a server.<br/>
/// The container contains all known information about a server as it was given depending on the flags passed.
/// </summary>
public sealed class ServerResult
{
	public required IPEndPoint EndPoint { get; init; }
	public required ServerQueryResponseType ServerChallengeResponse { get; init; }
	public required ServerResultState State { get; init; }

	public required string? Name { get; init; }
	public required string? Url { get; init; }
	public required string? Email { get; init; }
	public required string? MapName { get; init; }
	public required byte? MaxClients { get; init; }
	public required byte? MaxPlayers { get; init; }
	public required ReadOnlyCollection<PWad>? PwadCollection { get; init; }
	public required GameModeType? GameType { get; init; }
	public required bool? GameTypeInstagib { get; init; }
	public required bool? GameTypeBuckshot { get; init; }
	public required GameName? GameNameType { get; init; }
	public required string? Iwad { get; init; }
	public required bool? ForcePassword { get; init; }
	public required bool? ForceJoinPassword { get; init; }
	public required GameSkill? GameSkillType { get; init; }
	public required BotSkill? BotSkillType { get; init; }
	public required short? FragLimit { get; init; }
	public required short? TimeLimit { get; init; }
	public required short? TimeLeft { get; init; }
	public required short? DuelLimit { get; init; }
	public required short? PointLimit { get; init; }
	public required short? WinLimit { get; init; }
	public required float? TeamDamage { get; init; }
	public required byte? PlayerCount { get; init; }
	public required ReadOnlyCollection<Player>? PlayerDataCollection { get; init; }
	public required byte? TeamCount { get; init; }
	public required ReadOnlyCollection<Team>? TeamInfoCollection { get; init; }
	public required string? TestingServerName { get; init; }
	public required ReadOnlyCollection<int>? DmFlagCollection { get; init; }
	public required bool? SecuritySettings { get; init; }
	public required ReadOnlyCollection<string>? DehackedNameCollection { get; init; }
	public required string? Country { get; init; }
	public required string? GameModeName { get; init; }
	public required string? GameModeShortName { get; init; }

	/// <summary>
	/// Creates a new instance of <see cref="ServerResult"/> using the provided <see cref="ServerResultBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder containing the data for the <see cref="ServerResult"/>.</param>
	/// <param name="serverResultState">The final state of the data.</param>
	/// <returns>A <see cref="ServerResult"/>.</returns>
	internal static ServerResult Create(ServerResultBuilder builder, ServerResultState serverResultState)
	{
		// Url and email will both be empty if unset.
		// Rather than being empty we want `null`.
		var url = builder.url;
		if (string.IsNullOrEmpty(url))
		{
			url = null;
		}

		var email = builder.email;
		if (string.IsNullOrEmpty(email))
		{
			email = null;
		}

		var pwadEnumerable = builder.pwadNames?.Select((x, i) =>
			new PWad()
			{
				Name = x,
				Optional = builder.optionalPwadIndexes?.Contains((byte)i),
				Hash = builder.pwadHashes?[i],
			});

		var pwadCollection = pwadEnumerable != null ?
			new ReadOnlyCollection<PWad>(pwadEnumerable.ToArray()) :
			null;

		var gameType = (GameModeType?)builder.gameType;

		GameName? gameName = builder.gameName?.ToUpperInvariant() switch
		{
			"DOOM" => GameName.doom,
			"DOOM II" => GameName.doom2,
			"HERETIC" => GameName.heretic,
			"HEXEN" => GameName.hexen,
			"ERROR!" => GameName.error,
			null => null,
			_ => GameName.unknown,
		};

		// Game skill truncates any value above hardest into custom, as the actual skill name is never shared.
		GameSkill? gameSkill = builder.gameSkill != null ?
			builder.gameSkill > (byte)GameSkill.hardest ?
			GameSkill.custom :
			(GameSkill)builder.gameSkill :
			null;

		// Bot skill truncates any value above hardest into custom, as the actual skill name is never shared.
		BotSkill? botSkill = builder.botSkill != null ?
			builder.botSkill > (byte)BotSkill.hardest ?
			BotSkill.custom :
			(BotSkill)builder.botSkill :
			null;

		var playerEnumerable = builder.playerDataCollection?.Select(x => new Player()
		{
			Name = x.Name,
			ScoreCount = x.ScoreCount,
			Ping = x.Ping,
			IsSpectating = x.IsSpectating,
			IsBot = x.IsBot,
			Team = x.Team,
			PlayTime = x.PlayTime,
		});

		var playerCollection = playerEnumerable != null ?
			new ReadOnlyCollection<Player>(playerEnumerable.ToArray()) :
			null;

		var teamEnumerator = builder.teamInfoNumber != null ?
			Enumerable.Range(0, builder.teamInfoNumber.Value)
				.Select(i => new Team()
				{
					Name = builder.teamInfoNames?[i],
					Color = builder.teamInfoColors?[i],
					Score = builder.teamInfoScores?[i],
				}) :
			null;

		var teamCollection = teamEnumerator != null ?
			new ReadOnlyCollection<Team>(teamEnumerator.ToArray()) :
			null;

		// Testing server name should be `null` if empty.
		var testingServerName = builder.testingServerName;
		if (string.IsNullOrEmpty(testingServerName))
		{
			testingServerName = null;
		}

		var dmFlagCollection = builder.dmFlagCollection != null ?
			new ReadOnlyCollection<int>(builder.dmFlagCollection) :
			null;

		var dehackedNameCollection = builder.dehackedNameCollection != null ?
			new ReadOnlyCollection<string>(builder.dehackedNameCollection) :
			null;

		return new ServerResult()
		{
			EndPoint = builder.endPoint,
			ServerChallengeResponse = builder.response,
			State = serverResultState,
			Name = builder.name,
			Url = url,
			Email = email,
			MapName = builder.mapName,
			MaxClients = builder.maxClients,
			MaxPlayers = builder.maxPlayers,
			PwadCollection = pwadCollection,
			GameType = gameType,
			GameTypeInstagib = builder.gameTypeInstagib == 1,
			GameTypeBuckshot = builder.gameTypeBuckshot == 1,
			GameNameType = gameName,
			Iwad = builder.iwad,
			ForcePassword = builder.forcePassword == 1,
			ForceJoinPassword = builder.forceJoinPassword == 1,
			GameSkillType = gameSkill,
			BotSkillType = botSkill,
			FragLimit = builder.fragLimit,
			TimeLimit = builder.timeLimit,
			TimeLeft = builder.timeLeft,
			DuelLimit = builder.duelLimit,
			PointLimit = builder.pointLimit,
			WinLimit = builder.winLimit,
			TeamDamage = builder.teamDamage,
			PlayerCount = builder.numPlayers,
			PlayerDataCollection = playerCollection,
			TeamCount = builder.teamInfoNumber,
			TeamInfoCollection = teamCollection,
			TestingServerName = testingServerName,
			DmFlagCollection = dmFlagCollection,
			SecuritySettings = builder.securitySettings,
			DehackedNameCollection = dehackedNameCollection,
			Country = builder.country,
			GameModeName = builder.gameModeName,
			GameModeShortName = builder.gameModeShortName,
		};
	}
}
