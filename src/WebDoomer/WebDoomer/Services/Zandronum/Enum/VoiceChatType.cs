namespace WebDoomer.Zandronum;

/// <summary>
/// The server's voice chat setting which is based on <c>sv_allowvoicechat</c>.
/// </summary>
public enum VoiceChatType : byte
{
	/// <summary>Voice chat is disabled completely.</summary>
	/// <remarks>Cvar value <c>0</c>.</remarks>
	none,

	/// <summary>Everyone can voice chat..</summary>
	/// <remarks>Cvar value <c>1</c>.</remarks>
	everyone,

	/// <summary>Players can only voice chat with their teammates.</summary>
	/// <remarks>Cvar value <c>2</c>.</remarks>
	teammates,

	/// <summary>Players and spectators chat separately.</summary>
	/// <remarks>Cvar value <c>3</c>.</remarks>
	separately,
}
