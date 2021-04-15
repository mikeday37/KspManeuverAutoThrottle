using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Keeps track of critical variables while burning, in order to estimate the amount of burn time remaining.
	/// </summary>
	public class BurnLog
	{
		public class Entry
		{
			public double UT;
			public float Throttle;
			public double RemainingDV;
		}

		public static readonly BurnLog Instance = new BurnLog();

		private BurnLog()
		{
			Reset();
		}

		const int entryCount = 3;
		readonly Entry[] entries = InitEntries();
		int count = 0;
		int head = 0;

		static Entry[] InitEntries()
		{
			var entries = new Entry[entryCount];
			for (int i = 0; i < entries.Length; i++)
				entries[i] = new Entry();
			return entries;
		}

		public void Reset()
		{
			count = 0;
			EstimatesValid = false;
		}

		public bool EstimatesValid { get; set; }
		public double EstimatedAcceleration { get; set; }
		public double EstimatedMaxAcceleration { get; set; }
		public double EstimatedBurnTimeRemainingAtCurrentThrottle { get; set; }

		public void RecordAndEstimate()
		{
			var cur = entries[head];
			cur.UT = KspVars.CurrentUT;
			cur.Throttle = KspVars.CurrentThrottle;
			cur.RemainingDV = KspVars.NextManeuverRemainingDeltaV;

			if (cur.Throttle == 0.0f)
			{
				Reset();
				return;
			}

			if (count < entryCount)
				count++;

			var prevIndex = (head + entryCount - 1) % entryCount;
			var prev = entries[prevIndex];

			if (count > 1)
			{
				EstimatedAcceleration = (prev.RemainingDV - cur.RemainingDV) / (cur.UT - prev.UT);
				EstimatedMaxAcceleration = EstimatedAcceleration / (double)prev.Throttle;
				EstimatedBurnTimeRemainingAtCurrentThrottle = cur.RemainingDV / EstimatedAcceleration;
				EstimatesValid = true;
			}
			else
				EstimatesValid = false;

			head = (1 + head) % entryCount;
		}
	}
}
