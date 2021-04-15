using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Encapsulates the master on/off switch for this plugin.  This tracks and manipulates state only.
	/// Does not update UI.
	/// </summary>
	public static class MasterSwitch
	{
		static readonly object myLock = new object();
		static bool _enabled = false;
		public static bool IsEnabled { get { lock (myLock) return _enabled; } }
		public static void Toggle() { lock (myLock) _enabled = !_enabled; }
		public static void Disable() { lock (myLock) _enabled = false; }
	}
}
