using TaskFlow.Models;

namespace TaskFlow.Services
{
    public interface IConfigurationService
    {
        Task<AppConfiguration> LoadConfigurationAsync();
        Task SaveConfigurationAsync();
        Task SaveConfigurationAsync(AppConfiguration configuration);
        string ConfigurationFilePath { get; }
    }
}