﻿using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace ManeuverAutoThrottle
{
	/// <summary>
	/// Handles the KSP UI for the button which toggles and shows the state of the master switch.
	/// </summary>
	public class AppLauncher
	{
		public static readonly AppLauncher Instance = new AppLauncher();
		private AppLauncher()
		{
		}

		ApplicationLauncherButton button = null;
		static Texture2D offTexture, onTexture;

		const string texturePathPrefix = @"LuxSublima/ManeuverAutoThrottle/Resources/";

		public void Awake()
		{
			offTexture = GameDatabase.Instance.GetTexture(texturePathPrefix + "LauncherOff", false);
			onTexture = GameDatabase.Instance.GetTexture(texturePathPrefix + "LauncherOn", false);

			GameEvents.onGUIApplicationLauncherReady.Add(Add);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(Destroy);
			GameEvents.onGUIApplicationLauncherUnreadifying.Add(Destroy);
		}

		void Add()
		{
			if (button == null)
			{
				button = ApplicationLauncher.Instance.AddModApplication(OnClick, OnClick, null, null, null, null,
					ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, offTexture);
				SetTexture();
			}
		}

		void Destroy(GameScenes scenes)
		{
			if (scenes != GameScenes.FLIGHT)
				Destroy();
		}

		public void Destroy()
		{
			MasterSwitch.Disable();
			GameEvents.onGUIApplicationLauncherReady.Remove(Add);
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(Destroy);
			if (button != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(button);
				button = null;
			}
		}

		void OnClick()
		{
			if (button != null)
			{
				MasterSwitch.Toggle();
				SetTexture();
			}
		}

		public void SetTexture()
		{
			if (button != null)
			{
				if (MasterSwitch.IsEnabled)
				{
					button.SetTrue(false);
					button.SetTexture(onTexture);
				}
				else
				{
					button.SetFalse(false);
					button.SetTexture(offTexture);
				}
			}
		}
	}
}
