using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// A helper class for reliably getting well-defined KSP game variables.
	/// </summary>
	public static class KspVars
	{
		public enum WarpStatus
		{
			None = 0,
			Fast = 1,
			Physics = 2,
			
			Unknown = 99,
		}

		/// <summary>
		/// Returns true if and only if any maneuver is planned for the active vessel.  This returns true even if the burn start time
		/// for that maneuver is in the past.
		/// </summary>
		public static bool IsManeuverPlanned {get{
			return (FlightGlobals.ActiveVessel?.patchedConicSolver?.maneuverNodes?.Count ?? 0) > 0;
		}}

		/// <summary>
		/// Returns the time at which the burn for the next planned maneuver should start for the active vessel, in "Universal Time" on Kerbin.
		/// If no maneuver is planned, NaN is returned.
		/// </summary>
		public static double NextManeuverBurnStartUT {get{
			var nodes = FlightGlobals.ActiveVessel?.patchedConicSolver?.maneuverNodes;
			if (nodes != null && nodes.Count > 0 && nodes[0] != null)
				return nodes[0].startBurnIn + CurrentUT;
			else
				return double.NaN;
		}}

		/// <summary>
		/// The time in seconds UT until the next maneuver burn should start.  Will be negative if the time to start
		/// has already passed.  Will return NaN if no maneuver is planned.
		/// </summary>
		public static double TimeToNextManeuverBurnStartUT {get{
			if (IsManeuverPlanned)
				return NextManeuverBurnStartUT - CurrentUT;
			else
				return double.NaN;
		}}

		/// <summary>
		/// Returns the total delta V required to complete the next planned maneuver for the active vessel, if any, in m/s.
		/// If no maneuver is planned, NaN is returned.
		/// </summary>
		public static double NextManeuverTotalDeltaV {get{
			var nodes = FlightGlobals.ActiveVessel?.patchedConicSolver?.maneuverNodes;
			if (nodes != null && nodes.Count > 0)
				return nodes[0]?.DeltaV.magnitude ?? double.NaN;
			else
				return double.NaN;
		}}

		/// <summary>
		/// Returns the current remaining delta V required to complete the next planned maneuver for the active vessel, if any, in m/s.
		/// If no maneuver is planned, NaN is returned.
		/// </summary>
		public static double NextManeuverRemainingDeltaV {get{
			var nodes = FlightGlobals.ActiveVessel?.patchedConicSolver?.maneuverNodes;
			if (nodes != null && nodes.Count > 0)
				return nodes[0]?.GetPartialDv().magnitude ?? double.NaN;
			else
				return double.NaN;
		}}

		/// <summary>
		/// Returns the burn vector for the next planned maneuver for the acctive vessel, if any.
		/// If no maneuver is planned, the zero vector is returned.
		/// </summary>
		public static Vector3d NextManeuverBurnVector {get{
			var nodes = FlightGlobals.ActiveVessel?.patchedConicSolver?.maneuverNodes;
			var orbit = FlightGlobals.ActiveVessel?.orbit;
			if (nodes != null && nodes.Count > 0 && orbit != null)
				return nodes[0]?.GetBurnVector(orbit) ?? Vector3d.zero;
			else
				return Vector3d.zero;
		}}

		/// <summary>
		/// Returns the current "Universal Time" on Kerbin (zero when a new game is started,
		/// steadily increasing from there except when the game is paused).
		/// 
		/// This time increases according to the passage of real time when the game is not paused, multiplied by CurrentTimeWarpFactor,
		/// but may be slowed if framerate drops due to heavy processing.
		/// </summary>
		public static double CurrentUT {get{
			return Planetarium.GetUniversalTime();
		}}

		/// <summary>
		/// Determines what kind of warp, if any, is currently in effect, or Unknown if not a known, tested condition.
		/// </summary>
		public static WarpStatus CurrentWarpStatus {get{
			switch (TimeWarp.WarpMode)
			{
				case TimeWarp.Modes.LOW:
					return WarpStatus.Physics;

				case TimeWarp.Modes.HIGH:
					if (CurrentTimeWarpFactor == 1.0f)
						return WarpStatus.None;
					else if (CurrentTimeWarpFactor > 1.0f)
						return WarpStatus.Fast;
					break;
			}

			return WarpStatus.Unknown;
		}}

		/// <summary>
		/// Returns the actual current multiplicative factor for the passage of time, regardless of current warp setting,
		/// and regardless whether its normal warp or physics warp.
		/// </summary>
		public static float CurrentTimeWarpFactor {get{
			return TimeWarp.CurrentRate;
		}}

		/// <summary>
		/// Returns the current value of the main throttle.  Return value is between 0.0f (off) and 1.0f (full throttle).
		/// </summary>
		public static float CurrentThrottle {get{
			return FlightInputHandler.state.mainThrottle;
		}}

		/// <summary>
		/// Gets the persistentId of the active vessel, if any, or 0 if none.
		/// </summary>
		public static uint ActiveVesselPersistentId {get{
			return FlightGlobals.ActiveVessel?.persistentId ?? 0;
		}}

		/// <summary>
		/// Returns the thrust vector for the current vessel, if any, which is the direction it would accelerate if throttle is
		/// positive and engines engaged under a normal configuration ("up" at launch).
		/// If there is no current vessel, the zero vector is returned.
		/// </summary>
		public static Vector3d ThrustVector {get{
			var a = FlightGlobals.ActiveVessel?.GetTransform()?.up ?? Vector3.zero;
			return new Vector3d(a.x, a.y, a.z);
		}}

		/// <summary>
		/// The angle between the active vessel's current forward vector and the burn vector for the next planned maneuver, if any.
		/// If there's no such vessel or maneuver the NaN is returned.
		/// </summary>
		public static double NextManeuverOffAngle {get{
			if (FlightGlobals.ActiveVessel == null || !IsManeuverPlanned)
				return double.NaN;
			return Vector3d.Angle(ThrustVector, NextManeuverBurnVector);
		}}

		/// <summary>
		/// Returns true if the active vessel can enable maneuver hold autopilot.
		/// </summary>
		public static bool CanEnableAutoPilotManeuverHold {get{
			var autoPilot = FlightGlobals.ActiveVessel?.Autopilot;
			return autoPilot.CanSetMode(VesselAutopilot.AutopilotMode.Maneuver);
		}}
	}
}
