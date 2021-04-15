# KspManeuverAutoThrottle
A small plugin for Kerbal Space Program to optionally set max thrust at maneuver start, and gradually lower throttle toward the end for an accurate burn.

UI is very simple - just a toggle button added to the application bar.  Plugin does nothing if the button is not enabled/checked.

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

## Installation

Copy the LuxSublima folder to your KSP "GameData" subfolder.  For example, in my case this results in the following destination files:

```
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\LICENSE.txt
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\ManeuverAutoThrottle.dll
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\Resources\LauncherOff.png
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\Resources\LauncherOn.png
```

"LuxSublima" is the parent folder I'll use for any future KSP mods I make.

## Known Issues

1. Requires the NavBall to be enabled (not minimized) in order to detect the burn start time.  I haven't found a way to reliably get this data that doesn't depend on the NavBall, even though the property I'm using does not seem related to the NavBall at all (ManeuverNode.startBurnIn).

2. Rarely, it will not successfully warp to the next maneuver node after completing the prior.  If this happens, just toggle the button to reset the logic, and it should be fine.

The results are NOT as accurate as MechJeb's automatic maneuvering, but they're good enough for very satisfactory results most of the time.  My goal was only to make it about as accurate as my own manual efforts.  I got tired of repeatedly staring intently at the same tiny bits of UI for the most critical timing in the game, and decided to automate it.   :-)

Enjoy!
