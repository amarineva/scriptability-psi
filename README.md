# ScriptAbility PSI 💊

A Windows web service for processing pharmaceutical prescription data from Pioneer Rx pharmacy software, featuring a built-in system tray monitor for easy management.

## Overview

<<<<<<< HEAD
ScriptAbility PSI receives XML-formatted prescription data, parses it, and generates encrypted label files for external printing systems. It runs as a Windows service combining Apache HTTP Server and PHP, with a convenient system tray application for monitoring and control.
=======
ScriptAbility PSI receives XML-formatted prescription data, parses it, and generates encrypted label files for ScriptAbility. It runs as a Windows service combining Apache HTTP Server and PHP.
>>>>>>> 66d34605b8ac78887de1082a10a73f185b196717

## Installation 🚀

### Prerequisites
- Windows 10/11 or Windows Server
- Administrator privileges 🔑
- Visual C++ Redistributable 2015-2022 (x64) - *automatically installed if needed*

### Install Using NSIS Installer (Recommended)

1. Compile the installer:
   ```batch
   Scripts\compile_installer.bat
   ```

2. Run the generated installer as Administrator:
   ```
   ScriptAbilityPSI_Setup_1.1.exe
   ```

The installer will:
- Install the application to `C:\ScriptAbilityPSI\` with organized directory structure
- Configure Windows service to start automatically
- Set up Windows Firewall rules for port 18450 🔥
- Install PSI Monitor system tray application (starts with Windows)
- Generate installation information with your PC's IP address
- Automatically install Visual C++ Redistributable if needed

## Usage

### Web Service
Once installed, the service is accessible at:
- `http://localhost:18450/receive.php` (local applications)
- `http://YOUR_IP:18450/receive.php` (remote applications)

The service processes XML POST requests from Pioneer Rx and generates encrypted `.lbl` files in the configured output directory.

### PSI Monitor System Tray 🖥️
The PSI Monitor provides convenient access to:
- **Service Control**: Start, stop, or restart the ScriptAbility PSI service
- **Log Access**: Quick access to Apache and PHP error logs
- **Status Monitoring**: Real-time service status in the system tray
- **System Information**: Diagnostic details for troubleshooting

Right-click the system tray icon to access all features.

## Configuration ⚙️

- **Output Directory**: Configurable during installation (default: `C:\Users\Public\Programs\ScriptAbility\Labels`)
- **Port**: 18450 (configured in Apache)
- **Logs**: Available in `C:\ScriptAbilityPSI\logs\` or via PSI Monitor tray menu
- **PSI Monitor**: Auto-starts with Windows (configurable during installation)

## Uninstallation

Use Windows "Add or Remove Programs" or run:
```
C:\ScriptAbilityPSI\uninstall.exe
```

## Troubleshooting 🔧

- **Service fails to start**: Check `C:\ScriptAbilityPSI\logs\error.log` or use PSI Monitor → Apache Error Logs
- **Port 18450 in use**: Check with `netstat -an | findstr 18450`
<<<<<<< HEAD
- **PSI Monitor not visible**: Restart `C:\ScriptAbilityPSI\ScriptAbilityPSI_Monitor.exe`
- **Service control issues**: Ensure PSI Monitor is running with proper privileges
- **Log files won't open**: PSI Monitor automatically handles file locking issues

## Directory Structure 📁

After installation:
```
C:\ScriptAbilityPSI\
├── ScriptAbilityPSI_Monitor.exe    # System tray monitor
├── post_install_info.html          # Installation info page
├── Scripts\                        # PowerShell configuration scripts
├── apache\                         # Apache HTTP Server
├── php\                            # PHP 8 runtime
├── htdocs\                         # Web application files
└── logs\                           # Application logs
```

## Development

See [CLAUDE.md](CLAUDE.md) for comprehensive development documentation, architecture details, and advanced troubleshooting.
=======
- **Installation issues**: Ensure Visual C++ Redistributable is installed
>>>>>>> 66d34605b8ac78887de1082a10a73f185b196717
