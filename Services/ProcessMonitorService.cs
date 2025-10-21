using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TaskFlow.Models;
using System.IO;

namespace TaskFlow.Services
{
    public class ProcessMonitorService : IProcessMonitorService, IDisposable
    {
        private readonly ILogger<ProcessMonitorService> _logger;
        private readonly IConfigurationService _configurationService;
        private Timer? _monitoringTimer;
        private bool _isMonitoring;

        public event EventHandler<MonitoredApplication>? ApplicationStatusChanged;

        public ProcessMonitorService(ILogger<ProcessMonitorService> logger, IConfigurationService configurationService)
        {
            _logger = logger;
            _configurationService = configurationService;
        }

        public async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;
            var config = await _configurationService.LoadConfigurationAsync();
            
            _monitoringTimer = new Timer(async _ => await MonitorApplicationsAsync(), 
                null, TimeSpan.Zero, TimeSpan.FromMilliseconds(config.MonitoringInterval));

            _logger.LogInformation("Process monitoring started");
        }

        public async Task StopMonitoringAsync()
        {
            if (!_isMonitoring) return;

            _isMonitoring = false;
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;

            _logger.LogInformation("Process monitoring stopped");
            await Task.CompletedTask;
        }

        public async Task<bool> StartApplicationAsync(MonitoredApplication application)
        {
            try
            {
                if (IsApplicationRunning(application))
                {
                    _logger.LogWarning($"Application {application.Name} is already running");
                    return true;
                }

                application.Status = ApplicationStatus.Starting;
                ApplicationStatusChanged?.Invoke(this, application);

                var startInfo = new ProcessStartInfo
                {
                    FileName = application.ExecutablePath,
                    Arguments = application.Arguments,
                    WorkingDirectory = string.IsNullOrWhiteSpace(application.WorkingDirectory) 
                        ? Path.GetDirectoryName(application.ExecutablePath) 
                        : application.WorkingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                var process = Process.Start(startInfo);
                
                if (process != null)
                {
                    application.ProcessId = process.Id;
                    application.LastStarted = DateTime.Now;
                    application.Status = ApplicationStatus.Running;
                    application.CurrentRestartAttempts = 0;
                    
                    _logger.LogInformation($"Started application {application.Name} (PID: {process.Id})");
                    ApplicationStatusChanged?.Invoke(this, application);
                    
                    await _configurationService.SaveConfigurationAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                application.Status = ApplicationStatus.Error;
                _logger.LogError(ex, $"Failed to start application {application.Name}");
                ApplicationStatusChanged?.Invoke(this, application);
            }

            return false;
        }

        public async Task<bool> StopApplicationAsync(MonitoredApplication application)
        {
            try
            {
                if (!IsApplicationRunning(application))
                {
                    application.Status = ApplicationStatus.Stopped;
                    ApplicationStatusChanged?.Invoke(this, application);
                    return true;
                }

                application.Status = ApplicationStatus.Stopping;
                ApplicationStatusChanged?.Invoke(this, application);

                var process = Process.GetProcessById(application.ProcessId);
                process.CloseMainWindow();

                // Wait for graceful shutdown
                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                    _logger.LogWarning($"Force killed application {application.Name}");
                }

                application.LastStopped = DateTime.Now;
                application.Status = ApplicationStatus.Stopped;
                application.ProcessId = 0;

                _logger.LogInformation($"Stopped application {application.Name}");
                ApplicationStatusChanged?.Invoke(this, application);
                
                await _configurationService.SaveConfigurationAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to stop application {application.Name}");
                application.Status = ApplicationStatus.Error;
                ApplicationStatusChanged?.Invoke(this, application);
            }

            return false;
        }

        public async Task<bool> RestartApplicationAsync(MonitoredApplication application)
        {
            _logger.LogInformation($"Restarting application {application.Name}");
            
            if (IsApplicationRunning(application))
            {
                if (!await StopApplicationAsync(application))
                    return false;

                // Wait a moment before restarting
                await Task.Delay(2000);
            }

            return await StartApplicationAsync(application);
        }

        public bool IsApplicationRunning(MonitoredApplication application)
        {
            if (application.ProcessId == 0) return false;

            try
            {
                var process = Process.GetProcessById(application.ProcessId);
                return !process.HasExited;
            }
            catch (ArgumentException)
            {
                // Process doesn't exist
                return false;
            }
        }

        public async Task UpdateApplicationStatusAsync(MonitoredApplication application)
        {
            var wasRunning = application.Status == ApplicationStatus.Running;
            var isRunning = IsApplicationRunning(application);

            if (wasRunning && !isRunning)
            {
                // Application crashed
                application.Status = ApplicationStatus.Stopped;
                application.LastStopped = DateTime.Now;
                application.ProcessId = 0;

                _logger.LogWarning($"Application {application.Name} has stopped unexpectedly");

                if (application.RestartOnCrash && application.CurrentRestartAttempts < application.MaxRestartAttempts)
                {
                    application.CurrentRestartAttempts++;
                    _logger.LogInformation($"Attempting to restart {application.Name} (Attempt {application.CurrentRestartAttempts}/{application.MaxRestartAttempts})");
                    
                    await Task.Delay(5000); // Wait before restart
                    await StartApplicationAsync(application);
                }
                else
                {
                    ApplicationStatusChanged?.Invoke(this, application);
                }
            }
            else if (!wasRunning && isRunning)
            {
                application.Status = ApplicationStatus.Running;
                ApplicationStatusChanged?.Invoke(this, application);
            }

            await Task.CompletedTask;
        }

        private async Task MonitorApplicationsAsync()
        {
            if (!_isMonitoring) return;

            try
            {
                var config = await _configurationService.LoadConfigurationAsync();
                
                foreach (var app in config.Applications.Where(a => a.IsEnabled))
                {
                    await UpdateApplicationStatusAsync(app);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application monitoring");
            }
        }

        public void Dispose()
        {
            _monitoringTimer?.Dispose();
        }
    }
}