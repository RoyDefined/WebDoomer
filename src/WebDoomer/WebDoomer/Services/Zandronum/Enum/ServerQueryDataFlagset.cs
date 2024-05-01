namespace WebDoomer.Zandronum;

/// <summary>
/// An enum for flag set 0 which can be used when querying a server for its data, which corresponds to the data found in this set. Presets exist that define multiple flags in a collection.<br/>
/// When querying for data, this is the exact order at which the data is returned, and therefore this enum can be iterated when parsing the server result.
/// </summary>
[Flags]
public enum ServerQueryDataFlagset0 : uint
{
	/// <summary>
	/// For no flags.
	/// </summary>
	none = 0,

	/// <summary>The name of the server.</summary>
	/// <remarks>Internal name is <c>SQF_NAME</c>.</remarks>
	name = 0x00000001,

	/// <summary>The associated website.</summary>
	/// <remarks>Internal name is <c>SQF_URL</c>.</remarks>
	url = 0x00000002,

	/// <summary>Contact address.</summary>
	/// <remarks>Internal name is <c>SQF_EMAIL</c>.</remarks>
	email = 0x00000004,

	/// <summary>Current map being played.</summary>
	/// <remarks>Internal name is <c>SQF_MAPNAME</c>.</remarks>
	mapName = 0x00000008,

	/// <summary>Maximum amount of clients who can connect to the server.</summary>
	/// <remarks>Internal name is <c>SQF_MAXCLIENTS</c>.</remarks>
	maxClients = 0x00000010,

	/// <summary>Maximum amount of players who can join the game (the rest must spectate).</summary>
	/// <remarks>Internal name is <c>SQF_MAXPLAYERS</c>.</remarks>
	maxPlayers = 0x00000020,

	/// <summary>PWADs loaded by the server.</summary>
	/// <remarks>Internal name is <c>SQF_PWADS</c>.</remarks>
	pwads = 0x00000040,

	/// <summary>Game type code.</summary>
	/// <remarks>Internal name is <c>SQF_GAMETYPE</c>.</remarks>
	gameType = 0x00000080,

	/// <summary>Game mode name.</summary>
	/// <remarks>Internal name is <c>SQF_GAMENAME</c>.</remarks>
	gameName = 0x00000100,

	/// <summary>The IWAD used by the server.</summary>
	/// <remarks>Internal name is <c>SQF_IWAD</c>.</remarks>
	iwad = 0x00000200,

	/// <summary>Whether or not the server enforces a password.</summary>
	/// <remarks>Internal name is <c>SQF_FORCEPASSWORD</c>.</remarks>
	forcePassword = 0x00000400,

	/// <summary>Whether or not the server enforces a join password.</summary>
	/// <remarks>Internal name is <c>SQF_FORCEJOINPASSWORD</c>.</remarks>
	forceJoinPassword = 0x00000800,

	/// <summary>The skill level on the server.</summary>
	/// <remarks>Internal name is <c>SQF_GAMESKILL</c>.</remarks>
	gameSkill = 0x00001000,

	/// <summary>The skill level of any bots on the server.</summary>
	/// <remarks>Internal name is <c>SQF_BOTSKILL</c>.</remarks>
	botSkill = 0x00002000,

	/// <summary>The values of dmflags, dmflags2 and compatflags. Use <c>SQF_ALL_DMFLAGS</c> (<c>allDmFlags</c>) instead.</summary>
	/// <remarks>Internal name is <c>SQF_DMFLAGS</c>.</remarks>
	[Obsolete("Deprecated by protocol.")]
	dmFlags = 0x00004000,

	/// <summary>Timelimit, fraglimit, etc.</summary>
	/// <remarks>Internal name is <c>SQF_LIMITS</c>.</remarks>
	limits = 0x00010000,

	/// <summary>Team damage factor.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMDAMAGE</c>.</remarks>
	teamDamage = 0x00020000,

	/// <summary>The scores of the red and blue teams. Use <c>SQF_TEAMINFO_*</c> (<c>teamInfo*</c>) instead.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMSCORES</c>.</remarks>
	[Obsolete("Deprecated by protocol.")]
	teamScores = 0x00040000,

	/// <summary>Amount of players currently on the server.</summary>
	/// <remarks>Internal name is <c>SQF_NUMPLAYERS</c>.</remarks>
	numPlayers = 0x00080000,

	/// <summary>Information of each player in the server.</summary>
	/// <remarks>Internal name is <c>SQF_PLAYERDATA</c>.</remarks>
	playerData = 0x00100000,

	/// <summary>Amount of teams available.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_NUMBER</c>.</remarks>
	teamInfoNumber = 0x00200000,

	/// <summary>Names of teams.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_NAME</c>.</remarks>
	teamInfoName = 0x00400000,

	/// <summary>RGB colors of teams.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_COLOR</c>.</remarks>
	teamInfoColor = 0x00800000,

	/// <summary>Scores of teams.</summary>
	/// <remarks>Internal name is <c>SQF_TEAMINFO_SCORE</c>.</remarks>
	teamInfoScore = 0x01000000,

	/// <summary>Whether or not the server is a testing server, also the name of the testing binary.</summary>
	/// <remarks>Internal name is <c>SQF_TESTING_SERVER</c>.</remarks>
	testingServer = 0x02000000,

	/// <summary>Used to retrieve the MD5 checksum of skulltag_data.pk3, now obsolete and returns an empty string instead.</summary>
	/// <remarks>Internal name is <c>SQF_DATA_MD5SUM</c>.</remarks>
	[Obsolete("Deprecated by protocol.")]
	md5 = 0x04000000,

	/// <summary>Values of various dmflags used by the server.</summary>
	/// <remarks>Internal name is <c>SQF_ALL_DMFLAGS</c>.</remarks>
	allDmFlags = 0x08000000,

	/// <summary>Security setting values (for now only whether the server enforces the master banlist).</summary>
	/// <remarks>Internal name is <c>SQF_SECURITY_SETTINGS</c>.</remarks>
	securitySettings = 0x10000000,

	/// <summary>Which PWADs are optional.</summary>
	/// <remarks>Internal name is <c>SQF_OPTIONAL_WADS</c>.</remarks>
	optionalWads = 0x20000000,

	/// <summary>List of DEHACKED patches loaded by the server.</summary>
	/// <remarks>Internal name is <c>SQF_DEH</c>.</remarks>
	dehacked = 0x40000000,

	/// <summary>Additional server information.</summary>
	/// <remarks>Internal name is <c>SQF_EXTENDED_INFO</c>.</remarks>
	extendedInfo = 0x80000000,

	/// <summary>
	/// Base details about the server that can be queried.
	/// </summary>
	baseDetails = name | url | mapName | maxClients | maxPlayers | pwads | iwad | forcePassword |
		forceJoinPassword | numPlayers | playerData | teamInfoNumber | securitySettings | optionalWads,

	/// <summary>
	/// All details about the server that can be queried.
	/// </summary>
	all = name | url | email | mapName | maxClients | maxPlayers | pwads | gameType | gameName |
		iwad | forcePassword | forceJoinPassword | gameSkill | botSkill | /*dmFlags |*/ limits | teamDamage |
		/*teamScores |*/ numPlayers | playerData | teamInfoNumber | teamInfoName | teamInfoColor | teamInfoScore |
		testingServer | /*md5 |*/ allDmFlags | securitySettings | optionalWads | dehacked | extendedInfo,
}

/// <summary>
/// An enum for flag set 1 which can be used when querying a server for its data, which corresponds to the data found in this set. Presets exist that define multiple flags in a collection.<br/>
/// When querying for data, this is the exact order at which the data is returned, and therefore this enum can be iterated when parsing the server result.
/// </summary>
[Flags]
public enum ServerQueryDataFlagset1 : uint
{
	/// <summary>
	/// For no flags.
	/// </summary>
	none = 0,

	/// <summary>The MD5 hashes of the server's loaded PWADs.</summary>
	/// <remarks>Internal name is <c>SQF2_PWAD_HASHES</c>.</remarks>
	pwadHashes = 0x00000001,

	/// <summary>The server's <c>ISO 3166-1 alpha-3</c> country code.</summary>
	/// <remarks>Internal name is <c>SQF2_COUNTRY</c>.</remarks>
	country = 0x00000002,

	/// <summary>The name of the server's current game mode.</summary>
	/// <remarks>Internal name is <c>SQF2_GAMEMODE_NAME</c>.</remarks>
	gameModeName = 0x00000004,

	/// <summary>The short name of the server's current game mode.</summary>
	/// <remarks>Internal name is <c>SQF2_GAMEMODE_SHORTNAME</c>.</remarks>
	gameModeShortName = 0x00000008,

	/// <summary>The server's voice chat setting.</summary>
	/// <remarks>Internal name is <c>SQF2_VOICECHAT</c>.</remarks>
	voiceChat = 0x00000010,

	/// <summary>
	/// All details about the server that can be queried.
	/// </summary>
	all = pwadHashes | country | gameModeName | gameModeShortName | voiceChat,
}
