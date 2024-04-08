namespace WebDoomer.Zandronum;

/// <summary>
/// The different gamemode types that can be played.
/// </summary>
public enum GameModeType : byte
{
	/// <remarks>Also listed as <c>GAMEMODE_COOPERATIVE</c> on the wiki.</remarks>
	cooperative,

	/// <remarks>Also listed as <c>GAMEMODE_SURVIVAL</c> on the wiki.</remarks>
	survival,

	/// <remarks>Also listed as <c>GAMEMODE_INVASION</c> on the wiki.</remarks>
	invasion,

	/// <remarks>Also listed as <c>GAMEMODE_DEATHMATCH</c> on the wiki.</remarks>
	deathmatch,

	/// <remarks>Also listed as <c>GAMEMODE_TEAMPLAY</c> on the wiki.</remarks>
	teamPlay,

	/// <remarks>Also listed as <c>GAMEMODE_DUEL</c> on the wiki.</remarks>
	duel,

	/// <remarks>Also listed as <c>GAMEMODE_TERMINATOR</c> on the wiki.</remarks>
	terminator,

	/// <remarks>Also listed as <c>GAMEMODE_LASTMANSTANDING</c> on the wiki.</remarks>
	lastManStanding,

	/// <remarks>Also listed as <c>GAMEMODE_TEAMLMS</c> on the wiki.</remarks>
	teamLms,

	/// <remarks>Also listed as <c>GAMEMODE_POSSESSION</c> on the wiki.</remarks>
	possession,

	/// <remarks>Also listed as <c>GAMEMODE_TEAMPOSSESSION</c> on the wiki.</remarks>
	teamPossession,

	/// <remarks>Also listed as <c>GAMEMODE_TEAMGAME</c> on the wiki.</remarks>
	teamgame,

	/// <remarks>Also listed as <c>GAMEMODE_CTF</c> on the wiki.</remarks>
	ctf,

	/// <remarks>Also listed as <c>GAMEMODE_ONEFLAGCTF</c> on the wiki.</remarks>
	oneFlagCtf,

	/// <remarks>Also listed as <c>GAMEMODE_SKULLTAG</c> on the wiki.</remarks>
	skulltag,

	/// <remarks>Also listed as <c>GAMEMODE_DOMINATION</c> on the wiki.</remarks>
	domination,
}
