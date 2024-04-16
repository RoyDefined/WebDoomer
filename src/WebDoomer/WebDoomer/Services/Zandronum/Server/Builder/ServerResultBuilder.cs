using System.Diagnostics;
using System.Globalization;
using System.Net;
using WebDoomer.Packets;

#pragma warning disable CS0618 // Type or member is obsolete

namespace WebDoomer.Zandronum;

/// <summary>
/// Represents the builder responsible for building the response data up from the server and ensuring the data is correctly parsed.
/// </summary>
internal sealed class ServerResultBuilder
{
	internal sealed record class PlayerData(
		string Name,
		short ScoreCount,
		short Ping,
		bool IsSpectating,
		bool IsBot,
		byte? Team,
		byte PlayTime);

	internal readonly IPEndPoint endPoint;
	internal ServerQueryResponseType response;

	internal int? time;
	internal string? version;

	internal string? name;
	internal string? url;
	internal string? email;
	internal string? mapName;
	internal byte? maxClients;
	internal byte? maxPlayers;
	internal string[]? pwadNames;
	internal byte? gameType;
	internal byte? gameTypeInstagib;
	internal byte? gameTypeBuckshot;
	internal string? gameName;
	internal string? iwad;
	internal byte? forcePassword;
	internal byte? forceJoinPassword;
	internal byte? gameSkill;
	internal byte? botSkill;
	internal short? fragLimit;
	internal short? timeLimit;
	internal short? timeLeft;
	internal short? duelLimit;
	internal short? pointLimit;
	internal short? winLimit;
	internal float? teamDamage;
	internal byte? numPlayers;
	internal PlayerData[]? playerDataCollection;
	internal byte? teamInfoNumber;
	internal string[]? teamInfoNames;
	internal int[]? teamInfoColors;
	internal short[]? teamInfoScores;
	internal string? testingServerName;
	internal int[]? dmFlagCollection;
	internal bool? securitySettings;
	internal byte[]? optionalPwadIndexes;
	internal string[]? dehackedNameCollection;
	internal string[]? pwadHashes;
	internal string? country;
	internal string? gameModeName;
	internal string? gameModeShortName;

	public ServerResultBuilder(
		IPEndPoint endPoint)
	{
		this.endPoint = endPoint;
	}

	/// <summary>
	/// Parses the packet into the builder.
	/// </summary>
	/// <param name="packet">The packet to parse.</param>
	/// <returns> Returns <see langword="true"/> if more packets are expected.</returns>
	public bool Parse(Packet packet)
	{
		try
		{
			this.response = (ServerQueryResponseType)packet.GetInt();

			// Packet was not a succesful response.
			if (this.response is not ServerQueryResponseType.challenge and not ServerQueryResponseType.segmentedChallenge)
			{
				return false;
			}

			var @continue = this.ParseHeaderData(packet);
			this.ParseServerData(packet);

			return @continue;
		}
		catch (Exception ex)
		{
			throw new ArgumentException($"Failed to parse packet into {nameof(ServerResultBuilder)}.", ex);
		}
	}

	private bool ParseHeaderData(Packet packet)
	{
		switch (this.response)
		{
			case ServerQueryResponseType.challenge:

				// Time and version. Both unused.
				this.time = packet.GetInt();
				this.version = packet.GetString(true);
				return false;

			case ServerQueryResponseType.segmentedChallenge:
				var segmentNumber = packet.GetByte();

				// Uncompressed size of the packet. Unused.
				_ = packet.GetShort();

				var segmentIndex = segmentNumber & ~(1 << 7);
				var @continue = (segmentNumber & (1 << 7)) == 0;

				// If the segment index is zero, this is the first packet.
				if (segmentIndex == 0)
				{
					// Time and version. Both unused.
					this.time = packet.GetInt();
					this.version = packet.GetString(true);
				}

				return @continue;

			default:
				throw new UnreachableException($"Response {this.response} is not a valid positive response.");
		}
	}

	private void ParseServerData(Packet packet)
	{
		var isSegmented = this.response == ServerQueryResponseType.segmentedChallenge;
		var fieldSet = -1;

		while (packet.UnreadBytes > 0)
		{
			// Determine next field set and flags.
			// In the first iteration, this is part of the initial response.
			// For every concurrent iteration, this is part of the field set returned, representing `SQFEXTENDEDINFO`.
			// If the response is not segmented the data does not contain this and instead we can just increment the flag.
			fieldSet = !isSegmented ? fieldSet + 1 : packet.GetByte();
			var flags = packet.GetUInt();

			try
			{
				switch (fieldSet)
				{
					case 0:
						this.HandleFieldSet0(packet, (ServerQueryDataFlagset0)flags, isSegmented);
						break;

					case 1:
						this.HandleFieldSet1(packet, (ServerQueryDataFlagset1)flags);
						break;

					default:
						throw new UnreachableException($"Invalid field set: {fieldSet}.");
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Exception during server data parsing.", ex);
			}
		}
	}

	private void HandleFieldSet0(Packet packet, ServerQueryDataFlagset0 flags, bool isSegmented)
	{
		if (flags.HasFlag(ServerQueryDataFlagset0.name))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.name, value, ServerQueryDataFlagset0.name.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.url))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.url, value, ServerQueryDataFlagset0.url.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.email))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.email, value, ServerQueryDataFlagset0.email.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.mapName))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.mapName, value, ServerQueryDataFlagset0.mapName.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.maxClients))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.maxClients, packet.GetByte(), ServerQueryDataFlagset0.maxClients.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.maxPlayers))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.maxPlayers, packet.GetByte(), ServerQueryDataFlagset0.maxPlayers.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.pwads))
		{
			this.ThrowIfNotNull(this.pwadNames, "pwad names");
			this.pwadNames = Enumerable.Range(0, packet.GetByte())
				.Select(x => packet.GetString(true))
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.gameType))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.gameType, packet.GetByte(), ServerQueryDataFlagset0.gameType.ToString());
			this.AssignVariableOrThrowIfNotNull(ref this.gameTypeInstagib, packet.GetByte(), ServerQueryDataFlagset0.gameType.ToString());
			this.AssignVariableOrThrowIfNotNull(ref this.gameTypeBuckshot, packet.GetByte(), ServerQueryDataFlagset0.gameType.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.gameName))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.gameName, value, ServerQueryDataFlagset0.gameName.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.iwad))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.iwad, value, ServerQueryDataFlagset0.iwad.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.forcePassword))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.forcePassword, packet.GetByte(), ServerQueryDataFlagset0.forcePassword.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.forceJoinPassword))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.forceJoinPassword, packet.GetByte(), ServerQueryDataFlagset0.forceJoinPassword.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.gameSkill))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.gameSkill, packet.GetByte(), ServerQueryDataFlagset0.gameSkill.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.botSkill))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.botSkill, packet.GetByte(), ServerQueryDataFlagset0.botSkill.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.dmFlags))
		{
			throw new NotSupportedException($"Flag {ServerQueryDataFlagset0.dmFlags} is deprecated by protocol.");
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.limits))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.fragLimit, packet.GetShort(), ServerQueryDataFlagset0.limits.ToString());
			this.AssignVariableOrThrowIfNotNull(ref this.timeLimit, packet.GetShort(), ServerQueryDataFlagset0.limits.ToString());

			if (this.timeLimit > 0)
			{
				this.AssignVariableOrThrowIfNotNull(ref this.timeLeft, packet.GetShort(), ServerQueryDataFlagset0.limits.ToString());
			}

			this.AssignVariableOrThrowIfNotNull(ref this.duelLimit, packet.GetShort(), ServerQueryDataFlagset0.limits.ToString());
			this.AssignVariableOrThrowIfNotNull(ref this.pointLimit, packet.GetShort(), ServerQueryDataFlagset0.limits.ToString());
			this.AssignVariableOrThrowIfNotNull(ref this.winLimit, packet.GetShort(), ServerQueryDataFlagset0.limits.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.teamDamage))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.teamDamage, packet.GetFloat(), ServerQueryDataFlagset0.teamDamage.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.teamScores))
		{
			throw new NotSupportedException($"Flag {ServerQueryDataFlagset0.teamScores} is deprecated by protocol.");
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.numPlayers))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.numPlayers, packet.GetByte(), ServerQueryDataFlagset0.teamDamage.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.playerData))
		{
			// The `numPlayers` flag should be parsed at this point.
			if (this.numPlayers == null)
			{
				throw new InvalidOperationException("Required number of players does not exist whilst parsing players.");
			}

			this.ThrowIfNotNull(this.playerDataCollection, "player info");
			this.playerDataCollection = new PlayerData[this.numPlayers.Value];

			var hasTeams = isSegmented ? packet.GetBool() : flags.HasFlag(ServerQueryDataFlagset0.teamInfoNumber);
			for (var i = 0; i < this.playerDataCollection.Length; ++i)
			{
				this.playerDataCollection[i] = new PlayerData(
					packet.GetString(true),
					packet.GetShort(),
					packet.GetShort(),
					packet.GetByte() == 1,
					packet.GetByte() == 1,
					hasTeams ? packet.GetByte() : null,
					packet.GetByte());
			}
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.teamInfoNumber))
		{
			this.AssignVariableOrThrowIfNotNull(ref this.teamInfoNumber, packet.GetByte(), ServerQueryDataFlagset0.teamDamage.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.teamInfoName))
		{
			// The `teamInfoNumber` flag should be parsed at this point.
			if (this.teamInfoNumber == null)
			{
				throw new InvalidOperationException("Required number of teams does not exist whilst parsing team names.");
			}

			this.ThrowIfNotNull(this.teamInfoNames, "team info names");
			this.teamInfoNames = Enumerable.Range(0, this.teamInfoNumber.Value)
				.Select(x => packet.GetString(true))
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.teamInfoColor))
		{
			// The `teamInfoNumber` flag should be parsed at this point.
			if (this.teamInfoNumber == null)
			{
				throw new InvalidOperationException("Required number of teams does not exist whilst parsing team colors.");
			}

			this.ThrowIfNotNull(this.teamInfoColors, "team info colors");
			this.teamInfoColors = Enumerable.Range(0, this.teamInfoNumber.Value)
				.Select(x => packet.GetInt())
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.teamInfoScore))
		{
			// The `teamInfoNumber` flag should be parsed at this point.
			if (this.teamInfoNumber == null)
			{
				throw new InvalidOperationException("Required number of teams does not exist whilst parsing team scores.");
			}

			this.ThrowIfNotNull(this.teamInfoScores, "team info colors");
			this.teamInfoScores = Enumerable.Range(0, this.teamInfoNumber.Value)
				.Select(x => packet.GetShort())
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.testingServer))
		{
			// First byte indicates if the server runs a testing binary.
			// This is ignored because the next string will be empty if it doesn't.
			_ = packet.GetByte();
			var value = packet.GetString(true);
			this.AssignVariableOrThrowIfNotNull(ref this.testingServerName, value, ServerQueryDataFlagset0.testingServer.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.md5))
		{
			throw new NotSupportedException($"Flag {ServerQueryDataFlagset0.md5} is deprecated by protocol.");
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.allDmFlags))
		{
			this.ThrowIfNotNull(this.dmFlagCollection, "dm flags");
			this.dmFlagCollection = Enumerable.Range(0, packet.GetByte())
				.Select(x => packet.GetInt())
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.securitySettings))
		{
			var value = packet.GetByte() == 1;
			this.AssignVariableOrThrowIfNotNull(ref this.securitySettings, value, ServerQueryDataFlagset0.securitySettings.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.optionalWads))
		{
			this.ThrowIfNotNull(this.optionalPwadIndexes, "optional pwad indexes");
			this.optionalPwadIndexes = Enumerable.Range(0, packet.GetByte())
				.Select(x => packet.GetByte())
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset0.dehacked))
		{
			this.ThrowIfNotNull(this.dehackedNameCollection, "dehacked names");
			this.dehackedNameCollection = Enumerable.Range(0, packet.GetByte())
				.Select(x => packet.GetString(true).Trim())
				.ToArray();
		}
	}

	private void HandleFieldSet1(Packet packet, ServerQueryDataFlagset1 flags)
	{
		if (flags.HasFlag(ServerQueryDataFlagset1.pwadHashes))
		{
			this.ThrowIfNotNull(this.pwadHashes, "pwad hashes");
			this.pwadHashes = Enumerable.Range(0, packet.GetByte())
				.Select(x => packet.GetString(true).Trim())
				.ToArray();
		}

		if (flags.HasFlag(ServerQueryDataFlagset1.country))
		{
			var valueArray = Enumerable.Range(0, 3)
				.Select(x => packet.GetChar())
				.ToArray();
			var value = new string(valueArray);
			this.AssignVariableOrThrowIfNotNull(ref this.country, value, ServerQueryDataFlagset1.country.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset1.gameModeName))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.gameModeName, value, ServerQueryDataFlagset1.gameModeName.ToString());
		}

		if (flags.HasFlag(ServerQueryDataFlagset1.gameModeShortName))
		{
			var value = packet.GetString(true).Trim();
			this.AssignVariableOrThrowIfNotNull(ref this.gameModeShortName, value, ServerQueryDataFlagset1.gameModeShortName.ToString());
		}
	}

	private void AssignVariableOrThrowIfNotNull<T>(ref T? variable, T value, string field)
	{
		if (variable != null)
		{
			throw new InvalidOperationException($"Failed to parse {field} as it has already been assigned.");
		}

		variable = value;
	}

	private void ThrowIfNotNull<T>(T? value, string field)
	{
		if (value != null)
		{
			throw new InvalidOperationException($"Failed to parse {field} as it has already been assigned.");
		}
	}

	internal ServerResult Build(ServerResultState serverResultState = ServerResultState.Success)
	{
		return ServerResult.Create(this, serverResultState);
	}
}
