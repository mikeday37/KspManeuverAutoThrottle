using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Helper for logging in a standard and convenient way.
	/// </summary>
	public static class LogUtility
	{
		public static void Log(string message)
		{
			Debug.Log($"[ManeuverAutoThrottle] (RT = {Time.realtimeSinceStartup:000000.00}, UT = {KspVars.CurrentUT:000000000000.00}) -- {message}");
		}

		public static void LogBurnEstimates()
		{
			LogUtility.Log($"Burn Estimates: Time Remaining (at current throttle) = {KspVars.EstimatedNextManeuverBurnTimeRemainingAtCurrentThrottle:0.##}, Current Acceleration = {KspVars.CurrentAcceleration:0.##}");
		}
	}
}
