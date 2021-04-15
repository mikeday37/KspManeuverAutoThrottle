using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Makes the key decisions for what to do and when, and does them,
	/// to implement the automatic throttling for maneuvers.
	/// </summary>
	public class Logic
	{
		public void Reset()
		{
			bool redrawRequired = MasterSwitch.IsEnabled;
			LastActiveVesselPersistentId = 0;
			AutoPilotSet = false;
			LastSetThrottle = float.NaN;
			MasterSwitch.Disable();
			State = LogicState.Idle;
			BurnLog.Instance.Reset();
			if (redrawRequired)
				RedrawAction?.Invoke();
		}

		public Action RedrawAction {get;set;}

		public static readonly Logic Instance = new Logic();

		private Logic()
		{
			LastActiveVesselPersistentId = 0;
			AutoPilotSet = false;
			LastSetThrottle = float.NaN;
		}

		public uint LastActiveVesselPersistentId {get; private set;}
		public bool AutoPilotSet {get; private set;}
		public float LastSetThrottle {get; private set;}

		public void Tick()
		{
			// handle reset on change vessel
			if (LastActiveVesselPersistentId != KspVars.ActiveVesselPersistentId)
				Reset();
			LastActiveVesselPersistentId = KspVars.ActiveVesselPersistentId;
			
			// don't allow the plugin to enable if there's no maneuver node
			if (MasterSwitch.IsEnabled && !KspVars.IsManeuverPlanned)
			{
				Reset();
				return;
			}

			// reset if we were in a non-idle logic state but the plugin is disabled
			if (!MasterSwitch.IsEnabled && State != LogicState.Idle)
			{
				Reset();
				return;
			}

			// if not both enabled and there's a maneuver node, there's nothing to do
			if (!(MasterSwitch.IsEnabled && KspVars.IsManeuverPlanned))
				return;

			// if not already set, enable autopilot
			if (!AutoPilotSet)
			{
				KspCommands.EnableAutoPilotToManeuverVector();
				AutoPilotSet = true;
			}

			// if warping state but we're no longer warping, enter waiting state
			if (State == LogicState.WarpingToManeuver && KspVars.CurrentWarpStatus != KspVars.WarpStatus.Fast)
				State = LogicState.WaitToStart;

			// get time to burn
			var timeToStartBurn = KspVars.NextManeuverBurnStartUT - KspVars.CurrentUT;

			// if state idle
			if (State == LogicState.Idle)
			{
				// if the time is greater than the padding, warp unless aim is off
				if (timeToStartBurn > Settings.WarpSecondsBeforeBurn && KspVars.NextManeuverOffAngle < Settings.MaxManeuverOffAnglePreWarp)
				{
					KspCommands.TimeWarpToUT(KspVars.NextManeuverBurnStartUT - Settings.WarpSecondsBeforeBurn);
					State = LogicState.WarpingToManeuver;
					return;
				}
				
				// otherwise, if time not arrived but within padding, wait
				if (timeToStartBurn > 0.0 && timeToStartBurn <= Settings.WarpSecondsBeforeBurn)
				{
					State = LogicState.WaitToStart;
					return;
				}
			}

			// if not burning but time has arrived, start the burn
			if (State != LogicState.Burning && timeToStartBurn <= 0.0)
			{
				// save the burn vector
				OriginalBurnVector = KspVars.NextManeuverBurnVector;

				// set throttle to max if not already set
				if (KspVars.CurrentThrottle == 0.0f)
					KspCommands.SetThrottle(1.0f);

				// set state to burning
				State = LogicState.Burning;
			}

			// if now burning, handle lowwering throttle
			if (State == LogicState.Burning)
			{
				// stop the burn if either the remaining delta V is below the setting,
				// or the burn vector has diverged too far from the original (which would mean we probably overshot the maneuver)
				bool stopDone = KspVars.NextManeuverRemainingDeltaV <= Settings.MinRemainingDeltaV;
				bool stopOffCourse = 
					KspVars.NextManeuverRemainingDeltaV > Settings.MinRemainingDeltaVForOffCourseCheck
					&& Vector3d.Angle(OriginalBurnVector, KspVars.NextManeuverBurnVector) >= Settings.MaxBurnVectorDrift;
				if (stopDone || stopOffCourse)
				{
					// in either condition, kill throttle
					KspCommands.SetThrottle(0.0f);

					// if off course but not done, reset
					if (stopOffCourse && !stopDone)
					{
						Reset();
						return;
					}

					// otherwise, delete the maneuver we just completed
					KspCommands.DeleteNextManeuverNode();

					// if there are no further maneuver nodes, reset
					if (!KspVars.IsManeuverPlanned)
					{
						Reset();
						return;
					}

					// otherwise go to the idle state but don't disable the plugin, so we'll perform the next maneuver
					State = LogicState.Idle;

					// but clear the set autopilot flag so we'll set it again next time around
					AutoPilotSet = false;

					return;
				}

				// finally, if still burning, lower throttle appropriately if we have good estimates
				if (BurnLog.Instance.EstimatesValid)
					for (int i = 0; i < Settings.LowerThrottleRamp.Length; i++)
						if (BurnLog.Instance.EstimatedBurnTimeRemainingAtCurrentThrottle <= Settings.LowerThrottleRamp[i].secondsRemaining
								&& KspVars.CurrentThrottle > (Settings.LowerThrottleRamp[i].maxThrottle + Settings.ThrottleCheckSafetyMargin))
							KspCommands.SetThrottle(Settings.LowerThrottleRamp[i].maxThrottle);
			}
		}

		public Vector3d OriginalBurnVector {get;set;}

		private LogicState _state = LogicState.Idle;
		public LogicState State
		{
			get {return _state;}
		
			set
			{
				if (_state == value)
					return;

				LogUtility.Log($"State Change:  From = {_state}, To = {value}");

				_state = value;
			}
		}

		public enum LogicState
		{
			Idle = 0,
			WarpingToManeuver = 1,
			WaitToStart = 2,
			Burning = 3,
			Aiming = 4
		}
	}
}
