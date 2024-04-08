namespace WebDoomer.Zandronum;

/// <summary>
/// Represents the state of the server result.
/// </summary>
public enum ServerResultState
{
	/// <summary>
	/// The data was succesfully parsed.
	/// </summary>
	Success,

	/// <summary>
	/// The data could not be parsed due to a timeout,
	/// </summary>
	TimeOut,

	/// <summary>
	/// The data was not, or partially parsed due to an error.
	/// </summary>
	Error,
}
