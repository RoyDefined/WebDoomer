using WebDoomer;
using WebDoomerConsole;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Microsoft.Extensions.DependencyInjection;
using WebDoomer.Zandronum;
using System.Net;
using WebDoomer.QZandronum;

// The purpose of this console application is:
// - Testing/showcasing how a server list is fetched from the master server.
// - Testing/showcasing how data from an individual server is fetched.
//
// This console application is the easiest way to fetch data without relying on the schedule that exists in the main API application.
// Be sure to select one of the configurations to correctly use this console application.

#if RELEASE
#error This console application was made for debugging and should not be compiled into a release build.
#endif

// Parse passed arguments.
CommandOptions? commandOptions = CommandOptions.Parse(args);
if (commandOptions == null)
{
	return;
}

#if DEBUG
// This allows Serilog to log any errors during initialization.
SelfLog.Enable(Console.Error.WriteLine);
#endif

// Set up configuration
var configurationBuilder = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", false, false);

var configuration = configurationBuilder.Build();

var logger = new LoggerConfiguration()
	.ReadFrom.Configuration(configuration)
	.CreateLogger();

// Static logger for developmental purposes or where DI is not accessible. Use the injected ILogger for anything else.
Log.Logger = logger;

// Main startup.
logger.Debug("Configuring service collection...");
var services = new ServiceCollection();
IServiceProvider serviceProvider;
try
{
	// Logging
	_ = services.AddLogging(builder => builder.AddSerilog(logger));

	// WebDoomer
	_ = services.AddWebDoomer(configuration, "ServerFetchOptions");

	serviceProvider = services.BuildServiceProvider();
}
catch (Exception ex)
{
	logger.Error(ex, "Error during initial builder setup.");
	return;
}

logger.Debug("Starting application...");

switch(commandOptions.FetchType)
{
	case FetchType.Master:
		{
			var masterServerService = commandOptions.EngineType == EngineType.Zandronum ?
				serviceProvider.GetRequiredService<IZandronumMasterServerService>() :
				serviceProvider.GetRequiredService<IQZandronumMasterServerService>();

			var masterServerAddress = commandOptions.MasterServerAddress;
			var masterServerport = commandOptions.MasterServerport;

			var masterResult = await masterServerService.GetMasterServerHostsAsync(masterServerAddress!, masterServerport!.Value);

			logger.Information("Finished fetching with response ({ResponseTypeInt}){ResponseType}. Timed out: {TimedOut}. Fetched a total of {Count} hosts.", (int)masterResult.ServerChallengeResponse, masterResult.ServerChallengeResponse, masterResult.TimedOut, masterResult.Hosts.Count);
			foreach (var host in masterResult.Hosts)
			{
				logger.Information("{Address}: {PortsJoined}.", host.Address, string.Join(", ", host.Ports));
			}
			break;
		}

	case FetchType.Server:
		{
			var serverService = commandOptions.EngineType == EngineType.Zandronum ?
				serviceProvider.GetRequiredService<IZandronumServerService>() :
				serviceProvider.GetRequiredService<IQZandronumServerService>();

			var protocolType = commandOptions.LauncherProtocolType;
			var serverIpAddress = commandOptions.ServerIpAddress;
			var serverPort = commandOptions.ServerPort;

			var serverResult = await serverService.GetServerDataAsync(serverIpAddress!, serverPort!.Value, protocolType!.Value, ServerQueryDataFlagset0.all, ServerQueryDataFlagset1.all);

			logger.Information("Response: {Value}.", serverResult.ServerChallengeResponse);
			logger.Information("State: {Value}.", serverResult.State);

			logger.Information("Ping: {Value}.", serverResult.Ping);
			logger.Information("Version: {Value}.", serverResult.Version);

			logger.Information("Name: {Value}.", serverResult.Name);
			logger.Information("Url: {Value}.", serverResult.Url);
			logger.Information("Email: {Value}.", serverResult.Email);
			logger.Information("Map name: {Value}.", serverResult.MapName);
			logger.Information("Max clients: {Value}.", serverResult.MaxClients);
			logger.Information("Max players: {Value}.", serverResult.MaxPlayers);
			logger.Information("Gamemode: {Value} ({ValueInt}).", serverResult.GameType, (int?)serverResult.GameType);
			logger.Information("InstagibEnabled: {Value}.", serverResult.GameTypeInstagib);
			logger.Information("BuckshotEnabled: {Value}.", serverResult.GameTypeBuckshot);
			logger.Information("Game name: {Value} ({ValueInt}).", serverResult.GameNameType, (int?)serverResult.GameNameType);
			logger.Information("IWad: {Value}.", serverResult.Iwad);
			logger.Information("Has Password: {Value}.", serverResult.ForcePassword);
			logger.Information("Has Join Password: {Value}.", serverResult.ForceJoinPassword);
			logger.Information("Game difficulty: {Value} ({ValueInt}).", serverResult.GameSkillType, (int?)serverResult.GameSkillType);
			logger.Information("Bot difficulty: {Value} ({ValueInt}).", serverResult.BotSkillType, (int?)serverResult.BotSkillType);
			logger.Information("Enforces Master Banlist: {Value}.", serverResult.SecuritySettings);
			logger.Information("Testing Binary: {Value}.", serverResult.TestingServerName);
			logger.Information("DMFlags: {Value}.", serverResult.DmFlagCollection?.ElementAt(0));
			logger.Information("DMFlags2: {Value}.", serverResult.DmFlagCollection?.ElementAt(1));
			logger.Information("ZADMFlags: {Value}.", serverResult.DmFlagCollection?.ElementAt(2));
			logger.Information("CompatFlags: {Value}.", serverResult.DmFlagCollection?.ElementAt(3));
			logger.Information("ZACompatFlags: {Value}.", serverResult.DmFlagCollection?.ElementAt(4));
			logger.Information("CompatFlags2: {Value}.", serverResult.DmFlagCollection?.ElementAt(5));
			logger.Information("Country: {Value}.", serverResult.Country);

			if (serverResult.PlayerDataCollection != null)
			{
				logger.Information("Players:");
				foreach (var player in serverResult.PlayerDataCollection)
				{
					logger.Information("    - {Name} (bot: {IsBot}) - (spectating: {IsSpectating}) - (Ping: {Ping}) - (Play time: {PlayTime})", player.Name, player.IsBot, player.IsSpectating, player.Ping, player.PlayTime);
				}
			}

			if (serverResult.PwadCollection != null)
			{
				logger.Information("PWads:");
				foreach (var pwad in serverResult.PwadCollection)
				{
					logger.Information("    - {Name} (Hash: {Hash}) - (Optional: {Optional})", pwad.Name, pwad.Hash, pwad.Optional);
				}
			}

			break;
		}

	default:
		break;
}

logger.Debug("Application was finished.");