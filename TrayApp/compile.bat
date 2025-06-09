@echo off
echo Compiling ScriptAbility PSI Tray Monitor...

REM Find the latest .NET Framework compiler (prefer 4.8.1, fallback to 4.0)
set CSC_PATH=""
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set CSC_PATH="%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
) else if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set CSC_PATH="%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
) else if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set CSC_PATH="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
) else if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set CSC_PATH="%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
) else (
    echo ERROR: .NET Framework compiler not found!
    echo Please install .NET Framework 4.8.1 or Visual Studio 2022.
    pause
    exit /b 1
)

echo Using compiler: %CSC_PATH%

REM Compile the application with modern C# 7.3 features
%CSC_PATH% /target:winexe /langversion:7.3 /reference:System.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.ServiceProcess.dll /reference:System.Core.dll /out:ScriptAbilityPSI_Monitor.exe ScriptAbilityPSI_Monitor.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Compilation successful!
    echo Output: ScriptAbilityPSI_Monitor.exe
    echo.
) else (
    echo.
    echo Compilation failed with error code %ERRORLEVEL%
    echo.
    exit /b %ERRORLEVEL%
)