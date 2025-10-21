using TaskFlow.Models;

namespace TaskFlow.Services
{
    public interface IApplicationManagementService
    {
        Task<List<MonitoredApplication>> GetApplicationsAsync();
        Task AddApplicationAsync(MonitoredApplication application);
        Task UpdateApplicationAsync(MonitoredApplication application);
        Task RemoveApplicationAsync(string applicationId);
        Task<MonitoredApplication?> GetApplicationAsync(string applicationId);
        Task<SystemInfo> GetSystemInfoAsync();
    }
}