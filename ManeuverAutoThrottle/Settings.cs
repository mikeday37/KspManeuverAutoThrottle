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
		public const double MinRemainingDeltaV = 0.02;
		public const float ThrottleCheckSafetyMargin = 0.002f;
		public const double RemainingDeltaVIncreaseDetectionSafety = 0.0001;
		public const double SecondsRemainingStabilityAssistThreshold = 2.0;

		public static readonly (double secondsRemaining, float maxThrottle)[] LowerThrottleRamp = new (double, float)[]{
			(2.0, 0.8f),
			(1.0, 0.4f),
			(0.8, 0.2f),
			(0.6, 0.1f),
			(0.4, 0.05f),
		};
	}
}
