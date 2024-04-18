using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebDoomer.Zandronum;

namespace WebDoomer.QZandronum;

internal sealed class QZandronumMasterServerService : ZandronumMasterServerService, IQZandronumMasterServerService
{
	public QZandronumMasterServerService(
		ILogger<QZandronumMasterServerService> logger,
		IOptionsMonitor<WebDoomerOptions> optionsMonitor)
		: base(logger, optionsMonitor)
	{
	}
}
