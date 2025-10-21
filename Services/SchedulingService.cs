using Microsoft.Extensions.Logging;
using TaskFlow.Models;

namespace TaskFlow.Services
{
    public class SchedulingService : ISchedulingService, IDisposable
    {
        private readonly ILogger<SchedulingService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IProcessMonitorService _processMonitorService;
        private Timer? _schedulingTimer;
        private bool _isScheduling;

        public event EventHandler<ScheduleTask>? TaskExecuted;

        public SchedulingService(
            ILogger<SchedulingService> logger,
            IConfigurationService configurationService,
            IProcessMonitorService processMonitorService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _processMonitorService = processMonitorService;
        }

        public async Task StartSchedulingAsync()
        {
            if (_isScheduling) return;

            _isScheduling = true;
            
            // Check for pending tasks every minute
            _schedulingTimer = new Timer(async _ => await CheckPendingTasksAsync(), 
                null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            _logger.LogInformation("Scheduling service started");
            await Task.CompletedTask;
        }

        public async Task StopSchedulingAsync()
        {
            if (!_isScheduling) return;

            _isScheduling = false;
            _schedulingTimer?.Dispose();
            _schedulingTimer = null;

            _logger.LogInformation("Scheduling service stopped");
            await Task.CompletedTask;
        }

        public async Task AddScheduleAsync(ScheduleTask schedule)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            var application = config.Applications.FirstOrDefault(a => a.Id == schedule.ApplicationId);
            
            if (application != null)
            {
                application.Schedules.Add(schedule);
                await _configurationService.SaveConfigurationAsync();
                _logger.LogInformation($"Added schedule {schedule.Name} for application {application.Name}");
            }
        }

        public async Task RemoveScheduleAsync(string scheduleId)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            
            foreach (var application in config.Applications)
            {
                var schedule = application.Schedules.FirstOrDefault(s => s.Id == scheduleId);
                if (schedule != null)
                {
                    application.Schedules.Remove(schedule);
                    await _configurationService.SaveConfigurationAsync();
                    _logger.LogInformation($"Removed schedule {schedule.Name}");
                    break;
                }
            }
        }

        public async Task UpdateScheduleAsync(ScheduleTask schedule)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            
            foreach (var application in config.Applications)
            {
                var existingSchedule = application.Schedules.FirstOrDefault(s => s.Id == schedule.Id);
                if (existingSchedule != null)
                {
                    var index = application.Schedules.IndexOf(existingSchedule);
                    application.Schedules[index] = schedule;
                    await _configurationService.SaveConfigurationAsync();
                    _logger.LogInformation($"Updated schedule {schedule.Name}");
                    break;
                }
            }
        }

        public async Task ExecuteStartupTasksAsync()
        {
            var config = await _configurationService.LoadConfigurationAsync();
            var startupTasks = new List<(MonitoredApplication app, ScheduleTask task)>();

            foreach (var application in config.Applications.Where(a => a.IsEnabled))
            {
                foreach (var schedule in application.Schedules.Where(s => s.IsEnabled && s.ScheduleType == ScheduleType.Startup))
                {
                    startupTasks.Add((application, schedule));
                }
            }

            // Sort by startup delay
            startupTasks.Sort((x, y) => x.app.StartupDelay.CompareTo(y.app.StartupDelay));

            foreach (var (app, task) in startupTasks)
            {
                if (app.StartupDelay > TimeSpan.Zero)
                {
                    _logger.LogInformation($"Delaying startup of {app.Name} for {app.StartupDelay}");
                    await Task.Delay(app.StartupDelay);
                }

                await ExecuteTaskAsync(app, task);
            }
        }

        public List<ScheduleTask> GetPendingTasks()
        {
            var pendingTasks = new List<ScheduleTask>();
            var config = _configurationService.LoadConfigurationAsync().Result;
            var now = DateTime.Now;

            foreach (var application in config.Applications.Where(a => a.IsEnabled))
            {
                foreach (var schedule in application.Schedules.Where(s => s.IsEnabled))
                {
                    if (schedule.NextExecution.HasValue && schedule.NextExecution.Value <= now)
                    {
                        pendingTasks.Add(schedule);
                    }
                }
            }

            return pendingTasks.OrderBy(t => t.NextExecution).ToList();
        }

        private async Task CheckPendingTasksAsync()
        {
            if (!_isScheduling) return;

            try
            {
                var pendingTasks = GetPendingTasks();
                
                foreach (var task in pendingTasks)
                {
                    var config = await _configurationService.LoadConfigurationAsync();
                    var application = config.Applications.FirstOrDefault(a => a.Id == task.ApplicationId);
                    
                    if (application != null)
                    {
                        await ExecuteTaskAsync(application, task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pending tasks");
            }
        }

        private async Task ExecuteTaskAsync(MonitoredApplication application, ScheduleTask task)
        {
            try
            {
                _logger.LogInformation($"Executing task {task.Name} ({task.Action}) for application {application.Name}");

                bool success = false;

                switch (task.Action)
                {
                    case TaskAction.Start:
                        success = await _processMonitorService.StartApplicationAsync(application);
                        break;
                    
                    case TaskAction.Stop:
                        success = await _processMonitorService.StopApplicationAsync(application);
                        break;
                    
                    case TaskAction.Restart:
                        success = await _processMonitorService.RestartApplicationAsync(application);
                        break;
                }

                if (success)
                {
                    task.LastExecuted = DateTime.Now;
                    TaskExecuted?.Invoke(this, task);
                    await _configurationService.SaveConfigurationAsync();
                    _logger.LogInformation($"Successfully executed task {task.Name}");
                }
                else
                {
                    _logger.LogError($"Failed to execute task {task.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing task {task.Name}");
            }
        }

        public void Dispose()
        {
            _schedulingTimer?.Dispose();
        }
    }
}