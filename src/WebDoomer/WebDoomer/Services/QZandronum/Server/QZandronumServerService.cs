using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using WebDoomer.Zandronum;

namespace WebDoomer.QZandronum;

internal sealed class QZandronumServerService : ZandronumServerService, IQZandronumServerService
{
	public QZandronumServerService(
		ILogger<QZandronumServerService> logger,
		IOptionsMonitor<WebDoomerOptions> serverFetchOptionsMonitor)
		: base(logger, serverFetchOptionsMonitor)
	{
	}

	/// <inheritdoc />
	public async override Task<ServerResult> GetServerDataAsync(IPEndPoint endPoint, LauncherProtocolType protocolType, ServerQueryDataFlagset0 flagset0, ServerQueryDataFlagset1 flagset1, CancellationToken cancellationToken)
	{
		// QZandronum does not support the new protocol.
		if (protocolType == LauncherProtocolType.NewProtocol)
		{
			throw new NotImplementedException($"Protocol {nameof(LauncherProtocolType.NewProtocol)} is not implemented by QZandronum.");
		}

		return await base.GetServerDataAsync(endPoint, protocolType, flagset0, flagset1, cancellationToken)
			.ConfigureAwait(false);
	}
}
