set _ksp_plugin_dest=%KspPluginRoot%
xcopy /e /i /y "%~dp0out\LuxSublima" "%_ksp_plugin_dest%\LuxSublima"
dir "%_ksp_plugin_dest%\LuxSublima\ManeuverAutoThrottle"