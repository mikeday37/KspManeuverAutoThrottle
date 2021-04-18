using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Constants that help fine tune the logic.
	/// </summary>
	public class Settings
	{
		public const bool EnableManeuverHold = true;
		public const double MaxAimErrorAngleWithAutoPilot = 0.01;
		public const double MaxAimErrorAngleWithoutAutoPilot = 2.0;
		public readonly static StabilizationSettings AimStabilization = new StabilizationSettings(
			minFixedUpdates: 5,
			minLateUpdates: 5,
			minUTPassed: 0.4
		);

		public const ulong MinWarpRetryLateUpdateDelay = 5;

		public const double FarWarpBurnStartMarginSeconds = 60.0;
		public readonly static StabilizationSettings FarWarpRestSettings = new StabilizationSettings(
			minFixedUpdates: 5,
			minLateUpdates: 5,
			minUTPassed: 0.2
		);

		public const double NearWarpBurnStartMarginSeconds = 5.0;
		public readonly static StabilizationSettings NearWarpRestSettings = new StabilizationSettings(
			minFixedUpdates: 5,
			minLateUpdates: 5,
			minUTPassed: 0.2
		);

		public const double ThrottleUpSeconds = 0.5;
		public const float InitialThrottleSetting = 0.1f;
		public const double MaxRemainingDeltaVGoal = 0.001;
		public const double RemainingDeltaVIncreaseDetectionSafety = 0.00001;
		public const float ThrottleCheckSafetyMargin = 0.0002f;
		public readonly static StabilizationSettings ThrottleZeroRestSettings = new StabilizationSettings(
			minFixedUpdates: 5,
			minLateUpdates: 5,
			minUTPassed: 0.4
		);

		public static readonly (double secondsRemaining, float maxThrottle)[] LowerThrottleRamp = new (double, float)[]{
			(2.0, 0.8f),
			(1.0, 0.4f),
			(0.7, 0.2f),
			(0.5, 0.1f),
			(0.4, 0.05f),
			(0.3, 0.02f),
			(0.2, 0.01f),
			(0.1, 0.005f),
		};
	}

	public class StabilizationSettings
	{
		public ulong MinFixedUpdates {get; private set;}
		public ulong MinLateUpdates {get; private set;}
		public double MinUTPassed {get; private set;}

		public StabilizationSettings(
			ulong minFixedUpdates,
			ulong minLateUpdates,
			double minUTPassed
		)
		{
			this.MinFixedUpdates = minFixedUpdates;
			this.MinLateUpdates = minLateUpdates;
			this.MinUTPassed = minUTPassed;
		}
	}
}
