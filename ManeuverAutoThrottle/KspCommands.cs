using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Helper class for reliably performing certain operations in KSP.
	/// </summary>
	public static class KspCommands
	{
		/// <summary>
		/// Time warps as fast as possible until the "Universal Time" on Kerbin equals the specified time.
		/// </summary>
		public static void TimeWarpToUT(double universalTime)
		{
			TimeWarp.fetch.WarpTo(universalTime);
		}

		/// <summary>
		/// Sets the main throttle.  Value will be clamped to between 0.0f (off) and 1.0f (full throttle).
		/// If the attempt throws an exception, the exception will be hidden.
		/// </summary>
		public static void SetThrottle(float throttle)
		{
			try
			{
				var prevThrottle = FlightInputHandler.state.mainThrottle;
				if (throttle > 1.0f)
					throttle = 1.0f;
				if (throttle < 0.0f)
					throttle = 0.0f;
				FlightInputHandler.state.mainThrottle = throttle;
				LogUtility.Log($"Set Throttle: {throttle:0.####} -- (Prev = {prevThrottle}, Cur = {FlightInputHandler.state.mainThrottle})");
			}
			catch (Exception ex)
			{
				LogUtility.Log($"SetThrottle({throttle}) threw an exception: {ex.Message ?? "(null)"}");
			}
		}

		/// <summary>
		/// Deletes the next planned maneuver node for the active vessel, if any, otherwise does nothing.
		/// </summary>
		public static void DeleteNextManeuverNode()
		{
			var solver = FlightGlobals.ActiveVessel?.patchedConicSolver;
			if (solver != null && solver.maneuverNodes.Count >= 1)
			{
				var node = solver.maneuverNodes[0];
				if (node != null)
				{
					node.DetachGizmo();
					node.RemoveSelf();
					solver.UpdateFlightPlan();
					LogUtility.Log("Deleted Maneuver Node.");
				}
			}
		}

		/// <summary>
		/// Enables autopilot to head towards the next planned maneuver for the active vessel, if any, otherwise does nothing.
		/// </summary>
		public static void EnableAutoPilotToManeuverVector()
		{
			if (KspVars.IsManeuverPlanned)
				FlightGlobals.ActiveVessel?.Autopilot?.Enable(VesselAutopilot.AutopilotMode.Maneuver);
		}
	}
}
