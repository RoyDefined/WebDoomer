using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDoomerApi.Scheduling;

/// <summary>
/// Represents the state of a scheduled event.
/// </summary>
internal enum ScheduledEventState
{
	/// <summary>
	/// Schedule is available to be invoked.
	/// </summary>
	available,

	/// <summary>
	/// Schedule is running.
	/// </summary>
	running,
}
