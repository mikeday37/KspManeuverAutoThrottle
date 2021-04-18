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
		public static readonly Logic Instance = new Logic();
		public Action RedrawAction {get;set;}

		private Logic()
		{
			ResetLogic();
		}

		/// <summary>
		/// Resets the entire logic state machine and the UI to be completely idle/disabled.
		/// </summary>
		public void Reset()
		{
			bool redrawRequired = MasterSwitch.IsEnabled;
			LastActiveVesselPersistentId = null;
			ResetLogic();
			MasterSwitch.Disable();
			if (redrawRequired)
				RedrawAction?.Invoke();
		}

		public LogicState CurrentState {get; private set;}
		public LogicState? NextState {get; private set;}
		public bool StateChanged {get; private set;}

		public ulong FixedUpdateCountInState {get; private set;}
		public ulong LateUpdateCountInState {get; private set;}
		public double StateAgeUT {get; private set;}
		public double StateEnteredUT {get; private set;}

		public uint? LastActiveVesselPersistentId {get; private set;}

		public bool FullAutoPilot {get; private set;}

		/// <summary>
		/// Resets the logic state machine without affecting UI.
		/// </summary>
		void ResetLogic()
		{
			CurrentState = LogicState.Idle;
			FullAutoPilot = false;
			ResetCurrentState();
		}

		/// <summary>
		/// Resets the current state, by clearing NextState, StateChanged, and the update counters,
		/// and resets StateAgeUT to zero and StateEnteredUT to current UT.
		/// </summary>
		void ResetCurrentState()
		{
			NextState = null;
			StateChanged = false;

			FixedUpdateCountInState = 0;
			LateUpdateCountInState = 0;
			StateAgeUT = 0;
			StateEnteredUT = KspVars.CurrentUT;
		}

		public void OnFixedUpdate()
		{
			// increaes fixed update count
			FixedUpdateCountInState++;
		}

		/// <summary>
		/// Implements the core state machine, handling common state transition logic and dispatching for specific states.
		/// </summary>
		public void OnLateUpdate()
		{
			// reset if vessel changed while enabled
			if (MasterSwitch.IsEnabled
					&& 
					(
						!LastActiveVesselPersistentId.HasValue
						|| LastActiveVesselPersistentId.Value != KspVars.ActiveVesselPersistentId
					)
				)
			{
				LogUtility.Log("Vessel Changed - Resetting.");
				Reset();
				return;
			}
			LastActiveVesselPersistentId = KspVars.ActiveVesselPersistentId;

			// if master switch is enabled without a maneuver,
			// and current state is not Done and next state is not Idle
			// reset and don't go any further
			if ((MasterSwitch.IsEnabled && !KspVars.IsManeuverPlanned)
				&& CurrentState != LogicState.Done && !(NextState.HasValue && NextState.Value == LogicState.Idle))
			{
				LogUtility.Log("Cannot engage - No maneuver planned - Resetting.");
				Reset();
				return;
			}
				
			// switch to idle state if in any other state while master switch is disabled
			if (CurrentState != LogicState.Idle && !MasterSwitch.IsEnabled)
				NextState = LogicState.Idle;

			// if transitioning state
			if (NextState.HasValue && NextState.Value != CurrentState)
			{
				// if going idle, do a full reset
				if (NextState.Value == LogicState.Idle)
				{
					LogUtility.Log("Going Idle - Resetting.");
					Reset();
					return;
				}
				else
				{
					// otherwise, set next as current, reset the state tracking vars, and set the StateChanged flag
					var priorState = CurrentState;
					CurrentState = NextState.Value;
					ResetCurrentState();
					StateChanged = true;
					LogUtility.Log($"State Changed --> {CurrentState}");
				}
			}
			else
			{
				// otherwise (when not transitioning state) clear the StateChanged flag and calculate state age
				StateChanged = false;
				StateAgeUT = KspVars.CurrentUT - StateEnteredUT;
			}

			// increment late update count
			LateUpdateCountInState++;

			// handle current state
			switch (CurrentState)
			{
				case LogicState.Idle: ImplementState_Idle(); break;

				case LogicState.FarAim: ImplementState_FarAim(); break;
				case LogicState.FarAimStabilize: ImplementState_FarAimStabilize(); break;

				case LogicState.FarWarpStart: ImplementState_FarWarpStart(); break;
				case LogicState.FarWarpWait: ImplementState_FarWarpWait(); break;
				case LogicState.FarWarpRest: ImplementState_FarWarpRest(); break;

				case LogicState.NearAim: ImplementState_NearAim(); break;
				case LogicState.NearAimStabilize: ImplementState_NearAimStabilize(); break;

				case LogicState.NearWarpStart: ImplementState_NearWarpStart(); break;
				case LogicState.NearWarpWait: ImplementState_NearWarpWait(); break;
				case LogicState.NearWarpRest: ImplementState_NearWarpRest(); break;
			
				case LogicState.Countdown: ImplementState_Countdown(); break;

				case LogicState.ThrottleUp: ImplementState_ThrottleUp(); break;
				case LogicState.ThrottleMax: ImplementState_ThrottleMax(); break;
				case LogicState.ThrottleDown: ImplementState_ThrottleDown(); break;
				case LogicState.ThrottleZero: ImplementState_ThrottleZero(); break;

				case LogicState.Done: ImplementState_Done(); break;

				case LogicState.Staging: ImplementState_Staging(); break;

				case LogicState.NextManeuver: ImplementState_NextManeuver(); break;
			}
		}

		void CommonStateLogic_SetNextStateToStart(bool verbose = true)
		{
			if (KspVars.CanEnableAutoPilotManeuverHold)
			{
				if (verbose)
					LogUtility.Log("Maneuver Hold capability detected - enabling full autopilot.");
				FullAutoPilot = true;
				NextState = LogicState.FarAim;
			}
			else
			{
				if (verbose)
					LogUtility.Log("Maneuver Hold capability NOT detected - proceeding without autopilot.");
				FullAutoPilot = false;
				NextState = LogicState.FarWarpStart;
			}

			CommonStateLogic_SkipAhead();
		}

		/// <summary>
		/// If the time remaining until burn is too low to support the current or next (if set) state,
		/// this method sets next state to the appropriate state.
		/// </summary>
		void CommonStateLogic_SkipAhead()
		{
			// to minimize delay in handling appropriate skip ahead, we work on NextState if set, otherwise CurrentState
			LogicState relevantState = NextState ?? CurrentState;

			// this method is only relevant if between states FarAim (inclusive) and Countdown (exclusive)
			bool relevant = relevantState >= LogicState.FarAim && relevantState < LogicState.Countdown;
			if (!relevant)
				return;

			// cache time left to start
			var timeLeft = KspVars.TimeToNextManeuverBurnStartUT;

			// handle skipping ahead to countdown
			if (relevantState < LogicState.Countdown && timeLeft <= Settings.NearWarpBurnStartMarginSeconds)
			{
				LogUtility.Log("Skipping ahead to Countdown...");
				NextState = LogicState.Countdown;
			}
			else
				// otherwise, if autopiloting (so near aim state is relevant), handle skipping far warp
				if (FullAutoPilot && relevantState < LogicState.NearAim && timeLeft <= Settings.FarWarpBurnStartMarginSeconds)
				{
					LogUtility.Log("Skipping ahead to NearAim...");
					NextState = LogicState.NearAim;
				}
		}

		void ImplementState_Idle()
		{
			if (MasterSwitch.IsEnabled && KspVars.IsManeuverPlanned)
			{
				LogUtility.Log("ManeuverAutoThrottle Engaged...");

				CommonStateLogic_SetNextStateToStart();
			}
		}

		bool AreStabilizationSettingsAchieved(StabilizationSettings stabilizationSettings)
		{
			return
				FixedUpdateCountInState >= stabilizationSettings.MinFixedUpdates
				&& LateUpdateCountInState >= stabilizationSettings.MinLateUpdates
				&& StateAgeUT >= stabilizationSettings.MinUTPassed;
		}

		public double MaxAimErrorAngle {get{return FullAutoPilot
			? Settings.MaxAimErrorAngleWithAutoPilot
			: Settings.MaxAimErrorAngleWithoutAutoPilot;}}

		void CommonStateLogic_Aim(LogicState aimStabilizationState)
		{
			if (StateChanged && Settings.EnableManeuverHold)
				KspCommands.EnableAutoPilotManeuverHold();

			if (KspVars.NextManeuverOffAngle <= MaxAimErrorAngle)
				NextState = aimStabilizationState;

			CommonStateLogic_SkipAhead();
		}

		void CommonStateLogic_AimStabilize(LogicState postAimState)
		{
			if (KspVars.NextManeuverOffAngle > MaxAimErrorAngle)
				ResetCurrentState();
			else if (AreStabilizationSettingsAchieved(Settings.AimStabilization))
				NextState = postAimState;

			CommonStateLogic_SkipAhead();
		}

		public ulong? LastWarpRetryLateUpdateCount {get; private set;}

		void CommonStateLogic_WarpStart(double burnStartMarginSeconds, LogicState warpWaitState, LogicState skippedWarpState)
		{
			// don't prevent first attempt to warp per entry into this state
			if (StateChanged)
				LastWarpRetryLateUpdateCount = null;

			if (KspVars.CurrentWarpStatus == KspVars.WarpStatus.Fast)
			{
				// just wait until the warp ends if already warping
				NextState = warpWaitState;
				return;
			}

			var timeTilStart = KspVars.TimeToNextManeuverBurnStartUT;

			// skip warp entirely if we're already within the burn start margin
			if (timeTilStart <= burnStartMarginSeconds)
			{
				LogUtility.Log($"Skipping ahead to {skippedWarpState}...");
				NextState = skippedWarpState;
			}
			else
			{
				// otherwise, start the warp, but don't retry during the same state unless sufficient updates have passed
				// (prevents spamming with "already warping" messages)
				if (!LastWarpRetryLateUpdateCount.HasValue || LateUpdateCountInState - LastWarpRetryLateUpdateCount.Value >= Settings.MinWarpRetryLateUpdateDelay)
				{
					KspCommands.TimeWarpToUT(KspVars.CurrentUT + timeTilStart - burnStartMarginSeconds);
					LastWarpRetryLateUpdateCount = LateUpdateCountInState;
				}

				// transition to wait state if actually warping
				if (KspVars.CurrentWarpStatus == KspVars.WarpStatus.Fast)
					NextState = warpWaitState;
			}
		}

		void CommonStateLogic_WarpWait(double burnStartMarginSeconds, LogicState restState)
		{
			if (!StateChanged
					&& KspVars.CurrentWarpStatus != KspVars.WarpStatus.Fast
					&& KspVars.TimeToNextManeuverBurnStartUT <= burnStartMarginSeconds
				)
				NextState = restState;
		}

		void CommonStateLogic_WarpRest(StabilizationSettings restSettings, LogicState nextState)
		{
			if (AreStabilizationSettingsAchieved(restSettings))
				NextState = nextState;
		}

		void ImplementState_FarAim()
		{
			CommonStateLogic_Aim(
				aimStabilizationState: LogicState.FarAimStabilize
			);
		}

		void ImplementState_FarAimStabilize()
		{
			CommonStateLogic_AimStabilize(
				postAimState: LogicState.FarWarpStart
			);
		}

		void ImplementState_FarWarpStart()
		{
			CommonStateLogic_WarpStart(
				burnStartMarginSeconds: Settings.FarWarpBurnStartMarginSeconds,
				warpWaitState: LogicState.FarWarpWait,
				skippedWarpState: FullAutoPilot ? LogicState.NearAim : LogicState.Countdown
			);
		}

		void ImplementState_FarWarpWait()
		{
			CommonStateLogic_WarpWait(
				burnStartMarginSeconds: Settings.FarWarpBurnStartMarginSeconds,
				restState: LogicState.FarWarpRest
			);
		}

		void ImplementState_FarWarpRest()
		{
			CommonStateLogic_WarpRest(
				restSettings: Settings.FarWarpRestSettings,
				nextState: FullAutoPilot ? LogicState.NearAim : LogicState.Countdown
			);
		}

		void ImplementState_NearAim()
		{
			CommonStateLogic_Aim(
				aimStabilizationState: LogicState.NearAimStabilize
			);
		}

		void ImplementState_NearAimStabilize()
		{
			CommonStateLogic_AimStabilize(
				postAimState: LogicState.NearWarpStart
			);
		}

		void ImplementState_NearWarpStart()
		{
			CommonStateLogic_WarpStart(
				burnStartMarginSeconds: Settings.NearWarpBurnStartMarginSeconds,
				warpWaitState: LogicState.NearWarpWait,
				skippedWarpState: LogicState.Countdown
			);
		}

		void ImplementState_NearWarpWait()
		{
			CommonStateLogic_WarpWait(
				burnStartMarginSeconds: Settings.NearWarpBurnStartMarginSeconds,
				restState: LogicState.NearWarpRest
			);
		}

		void ImplementState_NearWarpRest()
		{
			CommonStateLogic_WarpRest(
				restSettings: Settings.NearWarpRestSettings,
				nextState: LogicState.Countdown
			);
		}

		void ImplementState_Countdown()
		{
			if (KspVars.CurrentThrottle != 0.0)
			{
				LogUtility.Log("Throttle already engaged - regarding current setting as max.");
				NextState = LogicState.ThrottleMax;
				CommonStateLogic_Throttle();
			}
			else if (KspVars.TimeToNextManeuverBurnStartUT <= Settings.ThrottleUpSeconds * squareRootOfOneHalf)
			{
				TargetMaxThrottleUT = KspVars.NextManeuverBurnStartUT + (Settings.ThrottleUpSeconds * (1 - squareRootOfOneHalf));
				LastRemainingDeltaV = KspVars.NextManeuverRemainingDeltaV;
				KspCommands.SetThrottle(Settings.InitialThrottleSetting);
				NextState = LogicState.ThrottleUp;
				CommonStateLogic_Throttle();
			}
		}

		const double squareRootOfOneHalf = 0.707106781;

		public double TargetMaxThrottleUT {get; private set;}

		void ImplementState_ThrottleUp()
		{
			if (CommonStateLogic_Throttle())
				return;

			var secondsToMax = TargetMaxThrottleUT - KspVars.CurrentUT;

			if (secondsToMax <= 0)
			{
				if (KspVars.CurrentThrottle != 1.0f)
					KspCommands.SetThrottle(1.0f);
				NextState = LogicState.ThrottleMax;
			}
			else
			{
				var marginPortionFulfilled = (Settings.ThrottleUpSeconds - secondsToMax) / Settings.ThrottleUpSeconds;
				var newThrottle = (float)((1.0f - Settings.InitialThrottleSetting) * marginPortionFulfilled + (double)Settings.InitialThrottleSetting);
				if (newThrottle > 1.0f)
					newThrottle = 1.0f;
				if (newThrottle < 0.0f)
					newThrottle = 0.0f;
				KspCommands.SetThrottle(newThrottle, false);
			}
		}

		void ImplementState_ThrottleMax()
		{
			if (CommonStateLogic_Throttle())
				return;

			if (StateChanged && KspVars.CurrentThrottle != 1.0f)
				KspCommands.SetThrottle(1.0f);
		}

		void ImplementState_ThrottleDown()
		{
			CommonStateLogic_Throttle();
		}

		public double LastRemainingDeltaV {get; private set;}

		/// <summary>
		/// Checks if we should stop, slow down, or allow for staging.  If so, handles the appropriate throttling and state transition, and returns true.
		/// If we don't need to do any of that currently, returns false.
		/// </summary>
		bool CommonStateLogic_Throttle()
		{
			// see how much delta V remains
			var remainingDeltaV = KspVars.NextManeuverRemainingDeltaV;

			// see if we should stop
			bool remainingDeltaVIncreasing = (remainingDeltaV - LastRemainingDeltaV) > Settings.RemainingDeltaVIncreaseDetectionSafety;
			LastRemainingDeltaV = remainingDeltaV;
			bool reachedGoal = remainingDeltaV <= Settings.MaxRemainingDeltaVGoal;
			bool stop = remainingDeltaVIncreasing || reachedGoal;

			// stop if we must
			if (stop)
			{
				LogUtility.Log($"Stop: ReachedGoal = {reachedGoal}, RemainingDeltaVIncreasing = {remainingDeltaVIncreasing}");
				LogUtility.Log($"Final Remaining DeltaV: {remainingDeltaV}");
				KspCommands.SetThrottle(0);
				NextState = LogicState.ThrottleZero;
				return true;
			}

			// see if we should wait for staging
			if (KspVars.IsCurrentStageExhausted)
			{
				LogUtility.Log($"Stage fuel exhausted - manually stage to continue.");
				StateAfterStaging = CurrentState;
				NextState = LogicState.Staging;
				return true;
			}


			// otherwise slow down according to lower throttle ramp if within threshold
			var burnTimeRemainingAtCurrentThrottle = KspVars.EstimatedNextManeuverBurnTimeRemainingAtCurrentThrottle;
			if (!double.IsNaN(burnTimeRemainingAtCurrentThrottle))
			{
				for (int i = 0; i < Settings.LowerThrottleRamp.Length; i++)
					if (burnTimeRemainingAtCurrentThrottle <= Settings.LowerThrottleRamp[i].secondsRemaining
							&& KspVars.CurrentThrottle > (Settings.LowerThrottleRamp[i].maxThrottle + Settings.ThrottleCheckSafetyMargin))
					{
						LogUtility.LogBurnEstimates();
						KspCommands.SetThrottle(Settings.LowerThrottleRamp[i].maxThrottle);
						NextState = LogicState.ThrottleDown;
						return true;
					}
			}

			return false;
		}

		void ImplementState_ThrottleZero()
		{
			if (KspVars.CurrentThrottle != 0)
			{
				KspCommands.SetThrottle(0);
				ResetCurrentState();
				return;
			}

			if (AreStabilizationSettingsAchieved(Settings.ThrottleZeroRestSettings))
				NextState = LogicState.Done;
		}

		void ImplementState_Done()
		{
			if (FullAutoPilot && MasterSwitch.IsRepeatEnabled)
			{
				KspCommands.DeleteNextManeuverNode();

				if (KspVars.IsManeuverPlanned)
				{
					LogUtility.Log("Next maneuver...");
					NextState = LogicState.NextManeuver;
				}
				else
				{
					LogUtility.Log("No further maneuvers planned.");
					NextState = LogicState.Idle;
				}
			}
			else
			{
				LogUtility.Log("Disengaging auto-throttle for manual resolution.");
				NextState = LogicState.Idle;
			}
		}

		public LogicState StateAfterStaging {get; private set;}

		void ImplementState_Staging()
		{
			if (KspVars.CurrentThrottle > 0.0 && !KspVars.IsCurrentStageExhausted)
				NextState = StateAfterStaging;
		}

		void ImplementState_NextManeuver()
		{
			if (AreStabilizationSettingsAchieved(Settings.NextManeuverRestSettings))
				CommonStateLogic_SetNextStateToStart(false);
		}
	}
}
