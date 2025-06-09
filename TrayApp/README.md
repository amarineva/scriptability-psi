# ScriptAbility PSI Tray Monitor

A Windows system tray application for monitoring and controlling the ScriptAbility PSI service.

## Features

- **System Tray Icon**: Persistent icon in the Windows notification area
- **Service Control**: Start, stop, and restart the ScriptAbility PSI service
- **Log Access**: Quick access to Apache and PHP error logs
- **Real-time Status**: Automatic monitoring of service status with menu updates
- **Smart Menu**: Menu items are automatically enabled/disabled based on service state

## Menu Options

1. **Start Service** - Starts the ScriptAbility PSI service (disabled if already running)
2. **Stop Service** - Stops the ScriptAbility PSI service (disabled if not running)
3. **Restart Service** - Restarts the ScriptAbility PSI service (disabled if not running)
4. **Apache Error Logs** - Opens the Apache error log file in Notepad
5. **PHP Error Logs** - Opens the PHP error log file in Notepad

## Compilation

To compile the tray monitor application:

1. Open Command Prompt as Administrator
2. Navigate to the TrayApp directory
3. Run: `compile.bat`

The compiler will generate `ScriptAbilityPSI_Monitor.exe`

## Installation

1. Copy `ScriptAbilityPSI_Monitor.exe` to the main ScriptAbility PSI directory
2. Run the executable to start monitoring
3. The application will appear in the system tray with a right-click menu

## Requirements

- Windows with .NET Framework 4.0 or later
- ScriptAbility PSI service installed
- Administrator privileges for service control operations

## Technical Details

- **Language**: C# Windows Forms
- **Dependencies**: .NET Framework 4.0 (included with Windows 7+)
- **Service Name**: "ScriptAbilityPSI"
- **Log Paths**:
  - Apache: `C:\ScriptAbilityPSI\logs\error.log`
  - PHP: `C:\ScriptAbilityPSI\logs\php_errors.log`

## Integration

This tray monitor is designed to be included in the NSIS installer and can optionally start with Windows for continuous monitoring of the ScriptAbility PSI service.