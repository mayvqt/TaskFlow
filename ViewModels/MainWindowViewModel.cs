using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TaskFlow.Models;
using TaskFlow.Services;

namespace TaskFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IProcessMonitorService _processMonitorService;
        private readonly ISchedulingService _schedulingService;
        private readonly IApplicationManagementService _applicationManagementService;
        private readonly IConfigurationService _configurationService;
        
        private string _statusMessage = "Ready";
        private string _logContent = string.Empty;
        private DateTime _currentTime = DateTime.Now;
        private Timer? _clockTimer;

        public ObservableCollection<MonitoredApplication> Applications { get; } = new();
        public ObservableCollection<ScheduleTask> Schedules { get; } = new();

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string LogContent
        {
            get => _logContent;
            set => SetProperty(ref _logContent, value);
        }

        public DateTime CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public int RunningApplicationsCount => Applications.Count(a => a.Status == ApplicationStatus.Running);
        public int StoppedApplicationsCount => Applications.Count(a => a.Status == ApplicationStatus.Stopped);

        // Commands
        public ICommand AddApplicationCommand { get; }
        public ICommand StartApplicationCommand { get; }
        public ICommand StopApplicationCommand { get; }
        public ICommand RestartApplicationCommand { get; }
        public ICommand EditApplicationCommand { get; }
        public ICommand DeleteApplicationCommand { get; }
        public ICommand StartAllApplicationsCommand { get; }
        public ICommand StopAllApplicationsCommand { get; }
        public ICommand RefreshApplicationsCommand { get; }
        
        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }
        public ICommand RefreshSchedulesCommand { get; }
        
        public ICommand ShowSystemInfoCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand TestMonitoringCommand { get; }

        public MainWindowViewModel(
            ILogger<MainWindowViewModel> logger,
            IProcessMonitorService processMonitorService,
            ISchedulingService schedulingService,
            IApplicationManagementService applicationManagementService,
            IConfigurationService configurationService)
        {
            _logger = logger;
            _processMonitorService = processMonitorService;
            _schedulingService = schedulingService;
            _applicationManagementService = applicationManagementService;
            _configurationService = configurationService;

            // Initialize commands
            AddApplicationCommand = new RelayCommand(AddApplication);
            StartApplicationCommand = new RelayCommand(StartApplication, CanExecuteApplicationCommand);
            StopApplicationCommand = new RelayCommand(StopApplication, CanExecuteApplicationCommand);
            RestartApplicationCommand = new RelayCommand(RestartApplication, CanExecuteApplicationCommand);
            EditApplicationCommand = new RelayCommand(EditApplication, CanExecuteApplicationCommand);
            DeleteApplicationCommand = new RelayCommand(DeleteApplication, CanExecuteApplicationCommand);
            StartAllApplicationsCommand = new RelayCommand(StartAllApplications);
            StopAllApplicationsCommand = new RelayCommand(StopAllApplications);
            RefreshApplicationsCommand = new RelayCommand(RefreshApplications);
            
            AddScheduleCommand = new RelayCommand(AddSchedule);
            EditScheduleCommand = new RelayCommand(EditSchedule, CanExecuteScheduleCommand);
            DeleteScheduleCommand = new RelayCommand(DeleteSchedule, CanExecuteScheduleCommand);
            RefreshSchedulesCommand = new RelayCommand(RefreshSchedules);
            
            ShowSystemInfoCommand = new RelayCommand(ShowSystemInfo);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowAboutCommand = new RelayCommand(ShowAbout);
            TestMonitoringCommand = new RelayCommand(TestMonitoring);

            // Setup event handlers
            _processMonitorService.ApplicationStatusChanged += OnApplicationStatusChanged;

            // Initialize clock timer
            _clockTimer = new Timer(_ => CurrentTime = DateTime.Now, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            // Load initial data
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                StatusMessage = "Initializing...";
                
                await LoadApplicationsAsync();
                await LoadSchedulesAsync();
                
                await _processMonitorService.StartMonitoringAsync();
                await _schedulingService.StartSchedulingAsync();
                
                // Execute startup tasks
                await _schedulingService.ExecuteStartupTasksAsync();
                
                StatusMessage = "Ready";
                _logger.LogInformation("Application initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize application");
                StatusMessage = $"Initialization failed: {ex.Message}";
            }
        }

        private async Task LoadApplicationsAsync()
        {
            var applications = await _applicationManagementService.GetApplicationsAsync();
            
            Applications.Clear();
            foreach (var app in applications)
            {
                Applications.Add(app);
            }
            
            OnPropertyChanged(nameof(RunningApplicationsCount));
            OnPropertyChanged(nameof(StoppedApplicationsCount));
        }

        private async Task LoadSchedulesAsync()
        {
            var applications = await _applicationManagementService.GetApplicationsAsync();
            
            Schedules.Clear();
            foreach (var app in applications)
            {
                foreach (var schedule in app.Schedules)
                {
                    Schedules.Add(schedule);
                }
            }
        }

        private void OnApplicationStatusChanged(object? sender, MonitoredApplication application)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(RunningApplicationsCount));
                OnPropertyChanged(nameof(StoppedApplicationsCount));
                
                var statusMessage = $"Application {application.Name} status changed to {application.Status}";
                var logMessage = $"[{DateTime.Now:HH:mm:ss}] {statusMessage}";
                
                // Add crash information if applicable
                if (application.Status == ApplicationStatus.Stopped && application.LastCrashTime.HasValue)
                {
                    logMessage += $" - Total crashes: {application.TotalCrashes}";
                    if (application.CurrentRestartAttempts > 0)
                    {
                        logMessage += $" - Restart attempts: {application.CurrentRestartAttempts}/{application.MaxRestartAttempts}";
                    }
                }
                
                // Add process ID information
                if (application.ProcessId > 0)
                {
                    logMessage += $" - PID: {application.ProcessId}";
                }
                
                StatusMessage = statusMessage;
                LogContent += logMessage + "\n";
            });
        }

        // Application Commands
        private void AddApplication(object? parameter)
        {
            try
            {
                var dialog = new Views.AddApplicationDialog();
                if (dialog.ShowDialog() == true && dialog.Application != null)
                {
                    Applications.Add(dialog.Application);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _applicationManagementService.AddApplicationAsync(dialog.Application);
                            Application.Current.Dispatcher.Invoke(() => 
                            {
                                StatusMessage = $"Added application: {dialog.Application.Name}";
                                LogContent += $"[{DateTime.Now:HH:mm:ss}] Added application: {dialog.Application.Name}\n";
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to save application");
                            Application.Current.Dispatcher.Invoke(() => 
                            {
                                LogContent += $"[{DateTime.Now:HH:mm:ss}] ERROR saving application: {ex.Message}\n";
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add application");
                LogContent += $"[{DateTime.Now:HH:mm:ss}] ERROR adding application: {ex.Message}\n";
                StatusMessage = "Error adding application";
            }
        }

        private void StartApplication(object? parameter)
        {
            if (parameter is MonitoredApplication app)
            {
                _ = Task.Run(async () =>
                {
                    var success = await _processMonitorService.StartApplicationAsync(app);
                    Application.Current.Dispatcher.Invoke(() =>
                        StatusMessage = success ? $"Started {app.Name}" : $"Failed to start {app.Name}");
                });
            }
        }

        private void StopApplication(object? parameter)
        {
            if (parameter is MonitoredApplication app)
            {
                _ = Task.Run(async () =>
                {
                    var success = await _processMonitorService.StopApplicationAsync(app);
                    Application.Current.Dispatcher.Invoke(() =>
                        StatusMessage = success ? $"Stopped {app.Name}" : $"Failed to stop {app.Name}");
                });
            }
        }

        private void RestartApplication(object? parameter)
        {
            if (parameter is MonitoredApplication app)
            {
                _ = Task.Run(async () =>
                {
                    var success = await _processMonitorService.RestartApplicationAsync(app);
                    Application.Current.Dispatcher.Invoke(() =>
                        StatusMessage = success ? $"Restarted {app.Name}" : $"Failed to restart {app.Name}");
                });
            }
        }

        private void EditApplication(object? parameter)
        {
            if (parameter is MonitoredApplication app)
            {
                var dialog = new Views.EditApplicationDialog(app);
                if (dialog.ShowDialog() == true && dialog.Application != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _applicationManagementService.UpdateApplicationAsync(dialog.Application);
                            await Application.Current.Dispatcher.InvokeAsync(async () => await LoadApplicationsAsync());
                            StatusMessage = $"Updated application {dialog.Application.Name}";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update application");
                            Application.Current.Dispatcher.Invoke(() => 
                                StatusMessage = $"Failed to update application: {ex.Message}");
                        }
                    });
                }
            }
        }

        private void DeleteApplication(object? parameter)
        {
            if (parameter is MonitoredApplication app)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete application '{app.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _applicationManagementService.RemoveApplicationAsync(app.Id);
                            await Application.Current.Dispatcher.InvokeAsync(async () => await LoadApplicationsAsync());
                            StatusMessage = $"Deleted application {app.Name}";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete application");
                            Application.Current.Dispatcher.Invoke(() => 
                                StatusMessage = $"Failed to delete application: {ex.Message}");
                        }
                    });
                }
            }
        }

        private void StartAllApplications(object? parameter)
        {
            _ = Task.Run(async () =>
            {
                var count = 0;
                foreach (var app in Applications.Where(a => a.IsEnabled && a.Status != ApplicationStatus.Running))
                {
                    if (await _processMonitorService.StartApplicationAsync(app))
                        count++;
                }
                
                Application.Current.Dispatcher.Invoke(() =>
                    StatusMessage = $"Started {count} applications");
            });
        }

        private void StopAllApplications(object? parameter)
        {
            _ = Task.Run(async () =>
            {
                var count = 0;
                foreach (var app in Applications.Where(a => a.Status == ApplicationStatus.Running))
                {
                    if (await _processMonitorService.StopApplicationAsync(app))
                        count++;
                }
                
                Application.Current.Dispatcher.Invoke(() =>
                    StatusMessage = $"Stopped {count} applications");
            });
        }

        private void RefreshApplications(object? parameter)
        {
            _ = Task.Run(async () =>
            {
                await LoadApplicationsAsync();
                Application.Current.Dispatcher.Invoke(() => StatusMessage = "Applications refreshed");
            });
        }

        private bool CanExecuteApplicationCommand(object? parameter)
        {
            return parameter is MonitoredApplication;
        }

        // Schedule Commands
        private void AddSchedule(object? parameter)
        {
            var dialog = new AddScheduleDialog(Applications.ToList());
            if (dialog.ShowDialog() == true && dialog.Schedule != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _schedulingService.AddScheduleAsync(dialog.Schedule);
                        await Application.Current.Dispatcher.InvokeAsync(async () => await LoadSchedulesAsync());
                        StatusMessage = $"Added schedule {dialog.Schedule.Name}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to add schedule");
                        Application.Current.Dispatcher.Invoke(() => 
                            StatusMessage = $"Failed to add schedule: {ex.Message}");
                    }
                });
            }
        }

        private void EditSchedule(object? parameter)
        {
            if (parameter is ScheduleTask schedule)
            {
                var dialog = new AddScheduleDialog(Applications.ToList(), schedule);
                if (dialog.ShowDialog() == true && dialog.Schedule != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _schedulingService.UpdateScheduleAsync(dialog.Schedule);
                            await Application.Current.Dispatcher.InvokeAsync(async () => await LoadSchedulesAsync());
                            StatusMessage = $"Updated schedule {dialog.Schedule.Name}";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update schedule");
                            Application.Current.Dispatcher.Invoke(() => 
                                StatusMessage = $"Failed to update schedule: {ex.Message}");
                        }
                    });
                }
            }
        }

        private void DeleteSchedule(object? parameter)
        {
            if (parameter is ScheduleTask schedule)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete schedule '{schedule.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _schedulingService.RemoveScheduleAsync(schedule.Id);
                            await Application.Current.Dispatcher.InvokeAsync(async () => await LoadSchedulesAsync());
                            StatusMessage = $"Deleted schedule {schedule.Name}";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete schedule");
                            Application.Current.Dispatcher.Invoke(() => 
                                StatusMessage = $"Failed to delete schedule: {ex.Message}");
                        }
                    });
                }
            }
        }

        private void RefreshSchedules(object? parameter)
        {
            _ = Task.Run(async () =>
            {
                await LoadSchedulesAsync();
                Application.Current.Dispatcher.Invoke(() => StatusMessage = "Schedules refreshed");
            });
        }

        private bool CanExecuteScheduleCommand(object? parameter)
        {
            return parameter is ScheduleTask;
        }

        // Other Commands
        private void ShowSystemInfo(object? parameter)
        {
            var dialog = new Views.SystemInfoDialog(_applicationManagementService);
            dialog.ShowDialog();
        }

        private void ShowSettings(object? parameter)
        {
            var dialog = new SettingsDialog(_configurationService);
            dialog.ShowDialog();
        }

        private void ShowAbout(object? parameter)
        {
            var dialog = new AboutDialog();
            dialog.ShowDialog();
        }

        private void TestMonitoring(object? parameter)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    LogContent += $"[{DateTime.Now:HH:mm:ss}] Testing monitoring for all applications...\n";
                    
                    foreach (var app in Applications)
                    {
                        await _processMonitorService.UpdateApplicationStatusAsync(app);
                        LogContent += $"[{DateTime.Now:HH:mm:ss}] Checked {app.Name}: Status={app.Status}, PID={app.ProcessId}\n";
                    }
                    
                    Application.Current.Dispatcher.Invoke(() => StatusMessage = "Monitoring test completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring test");
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        StatusMessage = "Monitoring test failed";
                        LogContent += $"[{DateTime.Now:HH:mm:ss}] ERROR: {ex.Message}\n";
                    });
                }
            });
        }
    }

    // Placeholder dialog classes - these would be implemented as separate windows/dialogs
    public class AddApplicationDialog
    {
        public MonitoredApplication? Application { get; set; }
        
        public AddApplicationDialog(MonitoredApplication? application = null)
        {
            Application = application;
        }
        
        public bool? ShowDialog() => true; // Placeholder
    }

    public class AddScheduleDialog
    {
        public ScheduleTask? Schedule { get; set; }
        
        public AddScheduleDialog(List<MonitoredApplication> applications, ScheduleTask? schedule = null)
        {
            Schedule = schedule;
        }
        
        public bool? ShowDialog() => true; // Placeholder
    }

    public class SystemInfoDialog
    {
        public SystemInfoDialog(IApplicationManagementService service)
        {
        }
        
        public bool? ShowDialog() => true; // Placeholder
    }

    public class SettingsDialog
    {
        public SettingsDialog(IConfigurationService service)
        {
        }
        
        public bool? ShowDialog() => true; // Placeholder
    }

    public class AboutDialog
    {
        public bool? ShowDialog() => true; // Placeholder
    }
}