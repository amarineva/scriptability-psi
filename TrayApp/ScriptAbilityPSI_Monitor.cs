using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptAbilityPSI
{
    public partial class TrayMonitor : Form
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _contextMenu;
        private Timer _statusTimer;
        private ServiceController _service;
        
        private ToolStripMenuItem _startServiceItem;
        private ToolStripMenuItem _stopServiceItem;
        private ToolStripMenuItem _restartServiceItem;
        private ToolStripMenuItem _apacheLogsItem;
        private ToolStripMenuItem _phpLogsItem;
        private ToolStripMenuItem _aboutItem;
        private ToolStripMenuItem _exitItem;

        private const string ServiceName = "ScriptAbilityPSI";
        private const int StatusUpdateInterval = 5000; // 5 seconds
        
        // Dynamically determine log paths from installation location
        private static string GetApacheLogPath()
        {
            var possiblePaths = new[]
            {
                @"C:\ScriptAbilityPSI\logs\error.log",                    // Default installer location
                Path.Combine(Application.StartupPath, "logs", "error.log") // Relative to tray app location
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path)) return path;
            }
            
            return possiblePaths[0]; // Return default if none found
        }
        
        private static string GetPhpLogPath()
        {
            var possiblePaths = new[]
            {
                @"C:\ScriptAbilityPSI\logs\php_errors.log",                    // Default installer location  
                Path.Combine(Application.StartupPath, "logs", "php_errors.log") // Relative to tray app location
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path)) return path;
            }
            
            return possiblePaths[0]; // Return default if none found
        }

        public TrayMonitor()
        {
            InitializeComponent();
            CreateTrayIcon();
            InitializeService();
            StartStatusMonitoring();
        }

        private void InitializeComponent()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;
        }

        private void CreateTrayIcon()
        {
            // Create context menu using object initializer
            _contextMenu = new ContextMenuStrip();
            
            _startServiceItem = new ToolStripMenuItem("Start Service");
            _startServiceItem.Click += StartService_Click;
            
            _stopServiceItem = new ToolStripMenuItem("Stop Service");
            _stopServiceItem.Click += StopService_Click;
            
            _restartServiceItem = new ToolStripMenuItem("Restart Service");
            _restartServiceItem.Click += RestartService_Click;
            
            _apacheLogsItem = new ToolStripMenuItem("Apache Error Logs");
            _apacheLogsItem.Click += ApacheLogs_Click;
            
            _phpLogsItem = new ToolStripMenuItem("PHP Error Logs");
            _phpLogsItem.Click += PhpLogs_Click;
            
            _aboutItem = new ToolStripMenuItem("About / System Info");
            _aboutItem.Click += About_Click;
            
            _exitItem = new ToolStripMenuItem("Exit");
            _exitItem.Click += Exit_Click;

            // Add items to context menu
            _contextMenu.Items.AddRange(new ToolStripItem[]
            {
                _startServiceItem,
                _stopServiceItem,
                _restartServiceItem,
                new ToolStripSeparator(),
                _apacheLogsItem,
                _phpLogsItem,
                new ToolStripSeparator(),
                _aboutItem,
                _exitItem
            });

            // Create tray icon with improved initialization
            _trayIcon = new NotifyIcon
            {
                Text = "ScriptAbility PSI Monitor v1.0",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };
            
            // Load icon with better path resolution - check multiple possible locations
            var iconPaths = new[]
            {
                Path.Combine(Application.StartupPath, "icon.ico"),                    // Same directory as exe
                Path.Combine(Application.StartupPath, "..", "Icons", "icon.ico"),     // Development structure
                Path.Combine(Application.StartupPath, "Icons", "icon.ico")           // Alternative location
            };
            
            Icon appIcon = null;
            foreach (var iconPath in iconPaths)
            {
                if (File.Exists(iconPath))
                {
                    appIcon = new Icon(iconPath);
                    break;
                }
            }
            
            _trayIcon.Icon = appIcon ?? SystemIcons.Application;
        }

        private void InitializeService()
        {
            try
            {
                _service = new ServiceController(ServiceName);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error connecting to service: {ex.Message}", "Service Error");
            }
        }

        private void StartStatusMonitoring()
        {
            _statusTimer = new Timer
            {
                Interval = StatusUpdateInterval
            };
            _statusTimer.Tick += UpdateServiceStatus;
            _statusTimer.Start();
            
            // Initial status update
            UpdateServiceStatus(null, null);
        }

        private void UpdateServiceStatus(object sender, EventArgs e)
        {
            if (_service == null) return;

            try
            {
                _service.Refresh();
                var status = _service.Status;
                
                var isRunning = status == ServiceControllerStatus.Running;
                var isStopped = status == ServiceControllerStatus.Stopped;
                
                // Update menu items using modern approach
                UpdateMenuItemStates(isRunning, isStopped);
                
                // Update tooltip with interpolated string (C# 6+ equivalent for C# 7.3)
                _trayIcon.Text = $"ScriptAbility PSI - {status}";
            }
            catch (Exception)
            {
                // Service might not exist or access denied - disable all controls
                UpdateMenuItemStates(false, false);
                _trayIcon.Text = "ScriptAbility PSI - Service Unavailable";
            }
        }

        private void UpdateMenuItemStates(bool isRunning, bool isStopped)
        {
            _startServiceItem.Enabled = isStopped;
            _stopServiceItem.Enabled = isRunning;
            _restartServiceItem.Enabled = isRunning;
        }

        private async void StartService_Click(object sender, EventArgs e)
        {
            await ExecuteServiceActionAsync(() =>
            {
                if (_service?.Status == ServiceControllerStatus.Stopped)
                {
                    _service.Start();
                    _service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                }
            }, "starting");
        }

        private async void StopService_Click(object sender, EventArgs e)
        {
            await ExecuteServiceActionAsync(() =>
            {
                if (_service?.Status == ServiceControllerStatus.Running)
                {
                    _service.Stop();
                    _service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                }
            }, "stopping");
        }

        private async void RestartService_Click(object sender, EventArgs e)
        {
            await ExecuteServiceActionAsync(() =>
            {
                if (_service?.Status == ServiceControllerStatus.Running)
                {
                    _service.Stop();
                    _service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    _service.Start();
                }
            }, "restarting");
        }

        private async Task ExecuteServiceActionAsync(Action serviceAction, string actionName)
        {
            try
            {
                await Task.Run(serviceAction);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error {actionName} service: {ex.Message}", "Service Error");
            }
        }

        private void ApacheLogs_Click(object sender, EventArgs e) => 
            OpenLogFile(GetApacheLogPath(), "Apache Error Log");

        private void PhpLogs_Click(object sender, EventArgs e) => 
            OpenLogFile(GetPhpLogPath(), "PHP Error Log");

        private void About_Click(object sender, EventArgs e)
        {
            var apacheLogPath = GetApacheLogPath();
            var phpLogPath = GetPhpLogPath();
            
            var aboutInfo = $"ScriptAbility PSI Monitor v1.0\n\n" +
                          $"Application Information:\n" +
                          $"• Running from: {Application.StartupPath}\n" +
                          $"• Executable: {Application.ExecutablePath}\n\n" +
                          $"Service Information:\n" +
                          $"• Service Name: {ServiceName}\n" +
                          $"• Service Status: {GetServiceStatus()}\n\n" +
                          $"Log File Paths:\n" +
                          $"• Apache Log: {apacheLogPath}\n" +
                          $"  - Exists: {File.Exists(apacheLogPath)}\n" +
                          $"• PHP Log: {phpLogPath}\n" +
                          $"  - Exists: {File.Exists(phpLogPath)}\n\n" +
                          $"Directory Check:\n" +
                          $"• C:\\ScriptAbilityPSI\\logs exists: {Directory.Exists(@"C:\ScriptAbilityPSI\logs")}\n" +
                          $"• Files in that directory: {GetLogDirectoryFiles()}";

            MessageBox.Show(aboutInfo, "PSI Monitor - System Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GetServiceStatus()
        {
            try
            {
                return _service?.Status.ToString() ?? "Unknown";
            }
            catch
            {
                return "Error";
            }
        }

        private string GetLogDirectoryFiles()
        {
            try
            {
                var logDir = @"C:\ScriptAbilityPSI\logs";
                if (Directory.Exists(logDir))
                {
                    var files = Directory.GetFiles(logDir, "*.log");
                    return files.Length > 0 ? string.Join(", ", files.Select(Path.GetFileName)) : "None";
                }
                return "Directory not found";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private void OpenLogFile(string logPath, string logName)
        {
            var diagnosticInfo = $"Diagnostic Info:\n" +
                          $"• Application Path: {Application.StartupPath}\n" +
                          $"• Target Log Path: {logPath}\n" +
                          $"• File Exists: {File.Exists(logPath)}\n" +
                          $"• Directory Exists: {Directory.Exists(Path.GetDirectoryName(logPath))}";
            
            try
            {
                if (File.Exists(logPath))
                {
                    try
                    {
                        // Try different approaches to open the file
                        
                        // Method 1: Use shell execute (more compatible with file permissions)
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = logPath,
                                UseShellExecute = true,  // Let Windows handle the file association
                                Verb = "open"            // Explicitly request to open
                            }
                        };
                        process.Start();
                    }
                    catch (Exception shellEx)
                    {
                        // Method 2: Fallback to notepad with shell execute
                        try
                        {
                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "notepad.exe",
                                    Arguments = $"\"{logPath}\"",  // Quote the path
                                    UseShellExecute = true
                                }
                            };
                            process.Start();
                        }
                        catch (Exception notepadEx)
                        {
                            // Method 3: Copy file to temp and open
                            try
                            {
                                var tempFile = Path.Combine(Path.GetTempPath(), $"PSI_{logName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                                File.Copy(logPath, tempFile, true);
                                
                                var process = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = tempFile,
                                        UseShellExecute = true
                                    }
                                };
                                process.Start();
                                
                                ShowWarningMessage($"Opened a copy of {logName} in temp folder.\n\nOriginal file may be locked by Apache.\n\nTemp file: {tempFile}", "File Opened as Copy");
                            }
                            catch (Exception copyEx)
                            {
                                ShowErrorMessage($"Multiple methods failed to open {logName}:\n\n1. Shell Execute: {shellEx.Message}\n2. Notepad: {notepadEx.Message}\n3. Copy to Temp: {copyEx.Message}\n\nThe file exists but may be locked by Apache or blocked by security software.", "File Access Error");
                            }
                        }
                    }
                }
                else
                {
                    // Show detailed diagnostic information
                    ShowErrorMessage($"{logName} file not found!\n\n{diagnosticInfo}", 
                                   $"{logName} - Diagnostic Information");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error accessing {logName}:\n\n{ex.Message}\n\n{diagnosticInfo}", "File Access Error");
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Application.Exit();
        }

        // Helper methods for consistent UI messaging
        private void ShowBalloonTip(string message) =>
            _trayIcon.ShowBalloonTip(3000, "ScriptAbility PSI", message, ToolTipIcon.Info);

        private static void ShowErrorMessage(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static void ShowWarningMessage(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected override void SetVisibleCore(bool value) => base.SetVisibleCore(false);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose resources using modern pattern
                _trayIcon?.Dispose();
                _statusTimer?.Dispose();
                _service?.Dispose();
                _contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayMonitor());
        }
    }
}