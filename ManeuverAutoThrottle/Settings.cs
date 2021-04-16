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
		public const double MaxManeuverOffAnglePreWarp = 1.0;
		public const double WarpSecondsBeforeBurn = 5.0;
		public const double MaxRemainingDeltaVGoal = 0.001;
		public const float ThrottleCheckSafetyMargin = 0.002f;
		public const double RemainingDeltaVIncreaseDetectionSafety = 0.00001;
		public const double SecondsRemainingStabilityAssistThreshold = 1.5;

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
}
