using TaskFlow.Models;

namespace TaskFlow.Services
{
    public interface IProcessMonitorService
    {
        event EventHandler<MonitoredApplication>? ApplicationStatusChanged;
        Task StartMonitoringAsync();
        Task StopMonitoringAsync();
        Task<bool> StartApplicationAsync(MonitoredApplication application);
        Task<bool> StopApplicationAsync(MonitoredApplication application);
        Task<bool> RestartApplicationAsync(MonitoredApplication application);
        bool IsApplicationRunning(MonitoredApplication application);
        Task UpdateApplicationStatusAsync(MonitoredApplication application);
    }
}