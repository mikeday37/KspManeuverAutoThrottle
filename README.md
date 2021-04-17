# KspManeuverAutoThrottle
A small plugin for Kerbal Space Program to automate the timing of maneuver burns.  Will quickly ramp up throttle at the proper time, then lower it toward the end of the burn for greater accuracy (much like I do manually).  Will also engage the KSP default "Warp to Next Maneuver" behavior before each burn.  Does not do any automatic staging nor maneuver editing.

This plugin is very lightweight and easy on memory.  It performs no allocations on the heap during any Update events, so it should not suffer from the stuttering/freezing problems encountered when using larger plugins that enable similar and more extensive functionality.

The UI is very simple - just a toggle button added to the application bar.  Plugin does nothing if the button is not enabled/checked.

You can uncheck the button at any time to disable this plugin's logic, but then you'll have to shut off throttle (if engaged) yourself.  This allows you to freely turn the auto throttle logic on and off when you want it, so you can still fine tune manually whenever you want.  You must always stage manually, but can leave the button enabled while you do so - it will pick up where it left off as soon as engines are activated with available fuel.

My goal was only to make it about as accurate as my own manual efforts, but just as reliable.  I got tired of repeatedly staring intently at the same tiny bits of UI for the most critical timing in the game, so I decided to automate it.   :-)  In the pursuit of making it reliable, it ended up becoming quite accurate in most cases and slightly more full-featured.

If your vessel supports Maneuver Hold, you're in for a treat, as it will also enable Auto Pilot, which gives the following enhancements:

1. Automatically enables Maneuver Hold before each burn.
2. Waits until the orientation of your vessel is highly accurate for the burn before warping.
3. Repeats aim-stabilization and another warp to decrease the amount of time you have to wait before each burn (KSP's default is about 60 seconds).
4. Automatically deletes the maneuver node when the burn is finished.
5. Proceeds automatically to the next maneuver node and repeats the whole process until no maneuver nodes remain.

All the above is accomplished with existing KSP features - the plugin doesn't engage any controls, nor modify any game variables, that a human player (or kerbal) could not.

Enjoy!

## Installation

Build the solution in Visual Studio, then copy the "out\LuxSublima" folder to your KSP "GameData" subfolder.  For example, in my case this results in the following destination files:

```
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\LICENSE.txt
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\ManeuverAutoThrottle.dll
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\Resources\LauncherOff.png
C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\GameData\LuxSublima\ManeuverAutoThrottle\Resources\LauncherOn.png
```

"LuxSublima" is the parent folder I'll use for any future KSP mods I make.

## Known Issues

WARNING: This is a work in progress.  I intend a lot more testing before considering it ready for release.

1. Currently requires the NavBall to be enabled (not minimized) in order to detect the burn start time.  I expect a way to work around this soon.

2. When in very low orbits around small bodies, the aim for the burn is sometimes less accurate.  This is due to the fact that the maneuver vector sometimes appears to wander during warp in very low orbits.

## Accuracy

The plugin aims to complete burns within 0.001 m/s of the planned Delta V, and often achieves that goal when Maneuver Hold is available.  Even in lower orbits where the maneuver vector drifts during warp, it often achieves an error of less than 0.1 m/s.  In rare cases (without Maneuver Hold), I've seen an error of about 0.3 m/s (and still often less) with manual piloting.
