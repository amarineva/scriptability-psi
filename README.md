# ScriptAbility PSI 💊

A Windows web service for processing pharmaceutical prescription data from Pioneer Rx pharmacy software.

## Overview

ScriptAbility PSI receives XML-formatted prescription data, parses it, and generates encrypted label files for ScriptAbility. It runs as a Windows service combining Apache HTTP Server and PHP.

## Installation 🚀

### Prerequisites
- Windows 10/11 or Windows Server
- Administrator privileges 🔑
- [Visual C++ Redistributable 2015-2022 (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe)

### Install Using NSIS Installer (Recommended)

1. Compile the installer:
   ```batch
   Scripts\compile_installer.bat
   ```

2. Run the generated installer as Administrator:
   ```
   ScriptAbilityPSI_Setup_1.0.0.exe
   ```

The installer will:
- Install the application to `C:\ScriptAbilityPSI\`
- Configure Windows service to start automatically
- Set up Windows Firewall rules for port 18450 🔥
- Generate installation information with your PC's IP address

## Usage

Once installed, the service is accessible at:
- `http://localhost:18450/receive.php` (local applications)
- `http://YOUR_IP:18450/receive.php` (remote applications)

The service processes XML POST requests from Pioneer Rx and generates encrypted `.lbl` files in the configured output directory.

## Configuration ⚙️

- **Output Directory**: Configurable during installation (default: `C:\Users\Public\Programs\ScriptAbility\Labels`)
- **Port**: 18450 (configured in Apache)
- **Logs**: Available in `C:\ScriptAbilityPSI\logs\`

## Uninstallation

Use Windows "Add or Remove Programs" or run:
```
C:\ScriptAbilityPSI\uninstall.exe
```

## Troubleshooting 🔧

- **Service fails to start**: Check `C:\ScriptAbilityPSI\logs\error.log`
- **Port 18450 in use**: Check with `netstat -an | findstr 18450`
- **Installation issues**: Ensure Visual C++ Redistributable is installed
