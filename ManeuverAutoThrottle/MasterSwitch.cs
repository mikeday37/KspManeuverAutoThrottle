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
		public static bool IsEnabled {get; private set;}
		public static void Toggle() {IsEnabled = ! IsEnabled;}
		public static void Disable() {IsEnabled = false;}

		public static bool IsRepeatEnabled {get; private set;}
		public static void ToggleRepeat() {IsRepeatEnabled = !IsRepeatEnabled;}
	}
}
