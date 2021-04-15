if "-%1-" == "--" (
	call "%~f0" "%~dp0ManeuverAutoThrottle\bin\debug\"
	goto :eof
)
set _publish_dir=%~dp0out\LuxSublima\ManeuverAutoThrottle
if exist %_publish_dir% rd /s /q %_publish_dir%
md %_publish_dir%
md %_publish_dir%\Resources
xcopy "%~f1Resources\*" "%_publish_dir%\Resources"
xcopy "%~f1ManeuverAutoThrottle.dll" "%_publish_dir%"
xcopy "%~dp0LICENSE.txt" "%_publish_dir%"