namespace WebDoomer.Zandronum;

/// <summary>
/// The different bot difficulties that can exist on a server.
/// </summary>
public enum BotSkill : byte
{
	/// <summary>
	/// "I Want My Mommy!"
	/// </summary>
	easiest,

	/// <summary>
	/// "I'm Allergic to Pain."
	/// </summary>
	easy,

	/// <summary>
	/// "Bring it on."
	/// </summary>
	average,

	/// <summary>
	/// "I Thrive off Pain."
	/// </summary>
	hard,

	/// <summary>
	/// "Nightmare!"
	/// </summary>
	/// <remarks>Also listed as [OMG] on the wiki.</remarks>
	hardest,

	/// <summary>
	/// For skills that go beyond the build in skill.
	/// </summary>
	custom,
}
