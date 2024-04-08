using CommandLine;
using CommandLine.Text;
using System.Net;
using EngineTypeEnum = WebDoomerConsole.EngineType;
using FetchTypeEnum = WebDoomerConsole.FetchType;
using LauncherProtocolTypeEnum = WebDoomer.Zandronum.LauncherProtocolType;

namespace WebDoomerConsole;

/// <summary>
/// Specifies the available commands that can be called.
/// </summary>
internal sealed class CommandOptions
{
	/// <summary>
	/// If `true`, the help parameter was passed.
	/// </summary>
	public bool Help { get; init; }

	[Option(longName: "engine", Required = true, HelpText = "Specify the target engine.")]
	public string? Engine { get; init; }

	[Option(longName: "fetch", Required = true, HelpText = "Specify the type of fetching that should happen.")]
	public string? Fetch { get; init; }

	[Option(longName: "protocol", Required = false, HelpText = "Specify the protocol to use when fetching servers.")]
	public string? Protocol { get; init; }

	[Option(longName: "masterserveraddress", Required = false, HelpText = "Specify the address of the master server to fetch from.")]
	public string? MasterServerAddress { get; init; }

	[Option(longName: "masterserverport", Required = false, HelpText = "Specify the port of the master server to fetch from.")]
	public int? MasterServerport { get; init; }

	[Option(longName: "serveraddress", Required = false, HelpText = "Specify the address of the server to fetch from.")]
	public string? ServerAddress { get; init; }

	[Option(longName: "serverport", Required = false, HelpText = "Specify the port of the server to fetch from.")]
	public int? ServerPort { get; init; }

	/// <summary>
	/// Returns a parsed console behaviour type for use in the application.
	/// </summary>
	public EngineTypeEnum EngineType => this.Engine switch
	{
		nameof(EngineTypeEnum.Zandronum) => EngineTypeEnum.Zandronum,
		nameof(EngineTypeEnum.QZandronum) => EngineTypeEnum.QZandronum,
		_ => EngineTypeEnum.Unknown,
	};

	/// <summary>
	/// Returns a parsed fetch type for use in the application.
	/// </summary>
	public FetchTypeEnum FetchType => this.Fetch switch
	{
		nameof(FetchTypeEnum.Master) => FetchTypeEnum.Master,
		nameof(FetchTypeEnum.Server) => FetchTypeEnum.Server,
		_ => FetchTypeEnum.Unknown,
	};

	/// <summary>
	/// Returns a parsed launcher protocol type for use in the application.
	/// </summary>
	public LauncherProtocolTypeEnum? LauncherProtocolType => this.Protocol switch
	{
		nameof(LauncherProtocolTypeEnum.OldProtocol) => LauncherProtocolTypeEnum.OldProtocol,
		nameof(LauncherProtocolTypeEnum.OldProtocolSegmented) => LauncherProtocolTypeEnum.OldProtocolSegmented,
		nameof(LauncherProtocolTypeEnum.NewProtocol) => LauncherProtocolTypeEnum.NewProtocol,
		_ => null,
	};

	/// <summary>
	/// Returns the parsed ip address of the server, or <see langword="null"/> if the address was not set.
	/// </summary>
	public IPAddress? ServerIpAddress => this.ServerAddress is {} ? IPAddress.Parse(this.ServerAddress) : null;

	public static CommandOptions? Parse(string[] args)
	{
		// The library forces a lot of various things so various parts of the code in here disables this.

		using var parser = new Parser(with => with.HelpWriter = null);
		var result = parser.ParseArguments<CommandOptions>(args);
		if (result is NotParsed<CommandOptions>)
		{
			// Help was requested.
			if (result.Errors.Count() == 1 && result.Errors.Single().Tag == ErrorType.HelpRequestedError)
			{
				var helpText = HelpText.AutoBuild(result, x => x, x => x);
				helpText.Heading = "";
				helpText.Copyright = "";
				Console.WriteLine(helpText);
				return null;
			}

			var builder = SentenceBuilder.Create();
			var error = HelpText.RenderParsingErrorsText(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
			Console.Error.WriteLine(error);
			return null;
		}

		var options = result.Value;

		// Validation

		// Engine must be set.
		if (options.EngineType == EngineTypeEnum.Unknown)
		{
			Console.Error.WriteLine($"{Environment.NewLine}Unknown engine was passed: {options.EngineType}.{Environment.NewLine}");
			return null;
		}

		// Fetch must be set.
		if (options.FetchType == FetchTypeEnum.Unknown)
		{
			Console.Error.WriteLine($"{Environment.NewLine}Unknown fetch was passed: {options.Fetch}.{Environment.NewLine}");
			return null;
		}

		// Protocol must be set if fetching is done for server.
		if (options.FetchType == FetchTypeEnum.Server && options.Protocol == null)
		{
			Console.Error.WriteLine($"{Environment.NewLine}Fetching for server requires a valid protocol.{Environment.NewLine}");
			return null;
		}

		// Master server address must be set if fetching is done for master.
		if (options.FetchType == FetchTypeEnum.Master && (options.MasterServerAddress == null || options.MasterServerport == null))
		{
			Console.Error.WriteLine($"{Environment.NewLine}Fetching for master requires a valid master server address and port.{Environment.NewLine}");
			return null;
		}

		// Server address must be set if fetching is done for server.
		if (options.FetchType == FetchTypeEnum.Server && (options.ServerAddress == null || options.ServerPort == null))
		{
			Console.Error.WriteLine($"{Environment.NewLine}Fetching for server requires a valid master server address and port.{Environment.NewLine}");
			return null;
		}

		return result.Value;
	}
}
