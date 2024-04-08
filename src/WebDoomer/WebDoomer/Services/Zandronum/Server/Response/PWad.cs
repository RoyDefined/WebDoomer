namespace WebDoomer.Zandronum;

/// <summary>
/// Represents a PWad.
/// </summary>
public sealed class PWad
{
	public required string Name { get; init; }
	public required bool? Optional { get; init; }
	public required string? Hash { get; init; }
}