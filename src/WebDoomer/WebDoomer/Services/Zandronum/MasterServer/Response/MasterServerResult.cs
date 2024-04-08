namespace WebDoomer.Zandronum;

public sealed class MasterServerResult
{
	private MasterServerResult()
	{
	}

	public required IReadOnlyCollection<HostIdentification> Hosts { get; init; }
	public required ServerChallengeResponseType ServerChallengeResponse { get; init; }
	public required bool TimedOut { get; init; }

	internal static MasterServerResult Create(MasterServerResultBuilder builder, bool timedOut)
	{
		return new MasterServerResult()
		{
			Hosts = builder.hosts.ToArray(),
			ServerChallengeResponse = builder.challengeResponse,
			TimedOut = timedOut
		};
	}
}
