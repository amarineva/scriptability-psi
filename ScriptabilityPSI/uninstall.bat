@echo off
echo Removing ScriptAbility PSI Service...

cd /d "%~dp0"

REM Stop service if running
echo Attempting to stop ScriptAbilityPSI service...
net stop ScriptAbilityPSI

REM Remove Windows service
echo Attempting to uninstall ScriptAbilityPSI service...
apache\bin\httpd.exe -k uninstall -n "ScriptAbilityPSI"

echo ScriptAbility PSI Service removal process completed.