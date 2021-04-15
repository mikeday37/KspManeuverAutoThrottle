using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Integrates this plugin into KSP.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class Plugin : MonoBehaviour
	{
		private AppLauncher launcher = null;

		public void Awake()
		{
			launcher = AppLauncher.Instance;
			launcher.Awake();
		}

		public void OnDestroy()
		{
			if (launcher != null)
			{
				Logic.Instance.Reset();
				launcher.Destroy();
			}
		}

		public void FixedUpdate()
		{
			// log burn variables if we're enabled and burning in order to get required estimates
			if (MasterSwitch.IsEnabled && KspVars.CurrentThrottle > 0.0f)
				BurnLog.Instance.RecordAndEstimate();
			else if (BurnLog.Instance.EstimatesValid) // otherwise if estimates were previously valid, reset the log
				BurnLog.Instance.Reset();
		}

		public void LateUpdate()
		{
			Logic.Instance.RedrawAction = () => launcher.SetTexture();
			Logic.Instance.Tick();
		}
	}
}
