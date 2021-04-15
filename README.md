# KspManeuverAutoThrottle
KSP Plugin to set max thrust at maneuver start, and gradually lower throttle toward end for accurate burn.

UI is very simple - just a toggle button added to the application bar.  Plugin does nothing if not enabled/checked.

When enabled, the following occurs:

1. If your craft supports it, will automatically enable "Hold Maneuver" autopilot mode.  If not, it will NOT help you aim toward the maneuver burn vector.  You'll have to do that yourself.
2. Once your craft is aiming within 1 degree of the maneuver indicator, if the maneuver start time is far in the future, it will time warp to just 5 seconds before the burn, so you don't have to wait as long or fine-tune the time warp.
3. At the proper time, it will set max throttle.
4. When there's only 2 seconds of burn left to complete the maneuver, it will automatically start progressively lowering the throttle to attempt a more accurate stop time.
5. When the burn has accomplished the intended DeltaV change within 0.02 m/s, it will shut off throttle and delete the maneuver node.
6. If there are any further maneuver nodes, the process repeats.  Otherwise, the plugin disables itself until you check the button again.

During step 5, there is also a failsafe abort:  If the realtime burn vector drifts 5 or more degrees from the original burn vector, it shuts throttle, does not delete the maneuver node, and disables the plugin.  This is to prevent continuing a flight plan if a burn is inaccurate.  In practice, I've only see this occur during development while tweaking the settings, but the failsafe remains just in case.

You can uncheck the button at any time to disable the above sequence, but then you'll have to shut off throttle (if engaged) yourself.  This allows you to freely turn the auto throttle logic on and off when you want it, so you can still fine tune manually whenever you want.

This mod is very lightweight and easy on memory.  The mod performs no allocations on the heap during any Update events, so this mod should not suffer from the stuttering/freezing problems encountered when using larger mods that enable this type of functionality.
