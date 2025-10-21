using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaskFlow.Models;
using System.IO;

namespace TaskFlow.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private AppConfiguration _currentConfiguration;
        private readonly string _configurationDirectory;

        public string ConfigurationFilePath { get; }

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            _configurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskFlow");
            ConfigurationFilePath = Path.Combine(_configurationDirectory, "config.json");
            _currentConfiguration = new AppConfiguration();
        }

        public async Task<AppConfiguration> LoadConfigurationAsync()
        {
            try
            {
                if (File.Exists(ConfigurationFilePath))
                {
                    var json = await File.ReadAllTextAsync(ConfigurationFilePath);
                    var config = JsonConvert.DeserializeObject<AppConfiguration>(json);
                    
                    if (config != null)
                    {
                        _currentConfiguration = config;
                        _logger.LogInformation($"Configuration loaded from {ConfigurationFilePath}");
                        return _currentConfiguration;
                    }
                }
                
                _logger.LogInformation("No configuration file found, using default configuration");
                return _currentConfiguration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration, using default");
                return _currentConfiguration;
            }
        }

        public async Task SaveConfigurationAsync()
        {
            await SaveConfigurationAsync(_currentConfiguration);
        }

        public async Task SaveConfigurationAsync(AppConfiguration configuration)
        {
            try
            {
                _currentConfiguration = configuration;
                
                // Ensure directory exists
                Directory.CreateDirectory(_configurationDirectory);

                var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
                await File.WriteAllTextAsync(ConfigurationFilePath, json);
                
                _logger.LogInformation($"Configuration saved to {ConfigurationFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration");
                throw;
            }
        }
    }
}