namespace TaskFlow.Models
{
    public class AppConfiguration
    {
        public List<MonitoredApplication> Applications { get; set; } = new();
        public bool StartMinimized { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public bool AutoStartApplications { get; set; } = true;
        public int MonitoringInterval { get; set; } = 5000; // milliseconds
        public bool ShowNotifications { get; set; } = true;
        public string LogLevel { get; set; } = "Information";
        public int MaxLogFiles { get; set; } = 10;
        public bool CheckForUpdates { get; set; } = true;
    }

    public class SystemInfo
    {
        public string MachineName { get; set; } = Environment.MachineName;
        public string UserName { get; set; } = Environment.UserName;
        public string OSVersion { get; set; } = Environment.OSVersion.ToString();
        public int ProcessorCount { get; set; } = Environment.ProcessorCount;
        public long TotalMemory { get; set; }
        public long AvailableMemory { get; set; }
        public double CpuUsage { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}