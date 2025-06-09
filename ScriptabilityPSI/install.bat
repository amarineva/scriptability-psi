@echo off
echo Installing ScriptAbility PSI Service...

cd /d "%~dp0"

REM Install Apache as Windows service
apache\bin\httpd.exe -k install -n "ScriptAbilityPSI" -f "conf\httpd.conf"

REM Configure service startup
sc config ScriptAbilityPSI start= auto
sc config ScriptAbilityPSI DisplayName= "ScriptAbility PSI Web Service"

REM Start the service
net start ScriptAbilityPSI

echo ScriptAbility PSI Service installed and started successfully!
echo Service URL: http://localhost:18450