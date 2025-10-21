using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TaskFlow.Models;

namespace TaskFlow.Services
{
    public class ApplicationManagementService : IApplicationManagementService
    {
        private readonly ILogger<ApplicationManagementService> _logger;
        private readonly IConfigurationService _configurationService;

        public ApplicationManagementService(
            ILogger<ApplicationManagementService> logger,
            IConfigurationService configurationService)
        {
            _logger = logger;
            _configurationService = configurationService;
        }

        public async Task<List<MonitoredApplication>> GetApplicationsAsync()
        {
            var config = await _configurationService.LoadConfigurationAsync();
            return config.Applications;
        }

        public async Task AddApplicationAsync(MonitoredApplication application)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            
            if (config.Applications.Any(a => a.Id == application.Id))
            {
                throw new InvalidOperationException($"Application with ID {application.Id} already exists");
            }

            config.Applications.Add(application);
            await _configurationService.SaveConfigurationAsync(config);
            
            _logger.LogInformation($"Added application {application.Name}");
        }

        public async Task UpdateApplicationAsync(MonitoredApplication application)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            var existingApp = config.Applications.FirstOrDefault(a => a.Id == application.Id);
            
            if (existingApp == null)
            {
                throw new InvalidOperationException($"Application with ID {application.Id} not found");
            }

            var index = config.Applications.IndexOf(existingApp);
            config.Applications[index] = application;
            
            await _configurationService.SaveConfigurationAsync(config);
            _logger.LogInformation($"Updated application {application.Name}");
        }

        public async Task RemoveApplicationAsync(string applicationId)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            var application = config.Applications.FirstOrDefault(a => a.Id == applicationId);
            
            if (application == null)
            {
                throw new InvalidOperationException($"Application with ID {applicationId} not found");
            }

            config.Applications.Remove(application);
            await _configurationService.SaveConfigurationAsync(config);
            
            _logger.LogInformation($"Removed application {application.Name}");
        }

        public async Task<MonitoredApplication?> GetApplicationAsync(string applicationId)
        {
            var config = await _configurationService.LoadConfigurationAsync();
            return config.Applications.FirstOrDefault(a => a.Id == applicationId);
        }

        public async Task<SystemInfo> GetSystemInfoAsync()
        {
            var systemInfo = new SystemInfo();
            
            try
            {
                // Get memory information
                var pc = new PerformanceCounter("Memory", "Available MBytes");
                systemInfo.AvailableMemory = (long)pc.NextValue() * 1024 * 1024; // Convert MB to bytes

                // Get total physical memory
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                systemInfo.TotalMemory = gcMemoryInfo.TotalAvailableMemoryBytes;

                // Get CPU usage (simple approximation)
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // First call returns 0
                await Task.Delay(100); // Wait a bit for accurate reading
                systemInfo.CpuUsage = cpuCounter.NextValue();

                systemInfo.LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system information");
            }

            return systemInfo;
        }
    }
}