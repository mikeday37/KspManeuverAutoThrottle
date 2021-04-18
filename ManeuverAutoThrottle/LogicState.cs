using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManeuverAutoThrottle
{
	public enum LogicState
	{
		Idle = 0,

		// NOTE: CommonStateLogic_SkipAhead() currently requires that all states from FarAim to CountDown
		//       have numerical values in sequence order.

		FarAim = 1010,
		FarAimStabilize = 1020,

		FarWarpStart = 2010,
		FarWarpWait = 2020,
		FarWarpRest = 2030,

		NearAim = 3010,
		NearAimStabilize = 3020,

		NearWarpStart = 4010,
		NearWarpWait = 4020,
		NearWarpRest = 4030,

		Countdown = 5010,

		ThrottleUp = 6010,
		ThrottleMax = 6020,
		ThrottleDown = 6030,
		ThrottleZero = 6040,

		Done = 7010,

		Staging = 100010,

		NextManeuver = 200010,
	}
}
