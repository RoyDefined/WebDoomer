using Microsoft.Extensions.Logging;
using WebDoomer.Zandronum;

namespace WebDoomer.QZandronum;

internal sealed class QZandronumMasterServerService : ZandronumMasterServerService, IQZandronumMasterServerService
{
	public QZandronumMasterServerService(
		ILogger<QZandronumMasterServerService> logger)
		: base(logger)
	{
	}
}
