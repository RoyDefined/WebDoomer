namespace WebDoomer.Zandronum;

/// <summary>
/// The different game difficulties that can exist on a server.
/// </summary>
public enum GameSkill : byte
{
	/// <summary>
	/// "I'm Too Young to Die"
	/// </summary>
	easiest,

	/// <summary>
	/// "Hey, Not Too Rough"
	/// </summary>
	easy,

	/// <summary>
	/// "Hurt Me Plenty"
	/// </summary>
	average,

	/// <summary>
	/// "Ultra-Violence"
	/// </summary>
	hard,

	/// <summary>
	/// "Nightmare!"
	/// </summary>
	/// <remarks>Also listed as [WTF?] on the wiki.</remarks>
	hardest,

	/// <summary>
	/// For skills that go beyond the build in skill.
	/// </summary>
	custom,
}
