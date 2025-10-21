using System.ComponentModel;
using System.Diagnostics;

namespace TaskFlow.Models
{
    public enum ApplicationStatus
    {
        Stopped,
        Running,
        Starting,
        Stopping,
        Error,
        Unknown
    }

    public enum ScheduleType
    {
        None,
        Startup,
        Interval,
        Daily,
        Weekly
    }

    public class MonitoredApplication : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _executablePath = string.Empty;
        private string _arguments = string.Empty;
        private string _workingDirectory = string.Empty;
        private bool _isEnabled = true;
        private ApplicationStatus _status = ApplicationStatus.Stopped;
        private DateTime? _lastStarted;
        private DateTime? _lastStopped;
        private int _processId;
        private TimeSpan _startupDelay = TimeSpan.Zero;
        private bool _restartOnCrash = true;
        private int _maxRestartAttempts = 3;
        private int _currentRestartAttempts;
        private DateTime? _lastCrashTime;
        private int _totalCrashes;
        private TimeSpan _totalUptime = TimeSpan.Zero;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string ExecutablePath
        {
            get => _executablePath;
            set { _executablePath = value; OnPropertyChanged(nameof(ExecutablePath)); }
        }

        public string Arguments
        {
            get => _arguments;
            set { _arguments = value; OnPropertyChanged(nameof(Arguments)); }
        }

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set { _workingDirectory = value; OnPropertyChanged(nameof(WorkingDirectory)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public ApplicationStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); OnPropertyChanged(nameof(StatusText)); }
        }

        public string StatusText => Status.ToString();

        public DateTime? LastStarted
        {
            get => _lastStarted;
            set { _lastStarted = value; OnPropertyChanged(nameof(LastStarted)); }
        }

        public DateTime? LastStopped
        {
            get => _lastStopped;
            set { _lastStopped = value; OnPropertyChanged(nameof(LastStopped)); }
        }

        public int ProcessId
        {
            get => _processId;
            set { _processId = value; OnPropertyChanged(nameof(ProcessId)); }
        }

        public TimeSpan StartupDelay
        {
            get => _startupDelay;
            set { _startupDelay = value; OnPropertyChanged(nameof(StartupDelay)); }
        }

        public bool RestartOnCrash
        {
            get => _restartOnCrash;
            set { _restartOnCrash = value; OnPropertyChanged(nameof(RestartOnCrash)); }
        }

        public int MaxRestartAttempts
        {
            get => _maxRestartAttempts;
            set { _maxRestartAttempts = value; OnPropertyChanged(nameof(MaxRestartAttempts)); }
        }

        public int CurrentRestartAttempts
        {
            get => _currentRestartAttempts;
            set { _currentRestartAttempts = value; OnPropertyChanged(nameof(CurrentRestartAttempts)); }
        }

        public DateTime? LastCrashTime
        {
            get => _lastCrashTime;
            set { _lastCrashTime = value; OnPropertyChanged(nameof(LastCrashTime)); }
        }

        public int TotalCrashes
        {
            get => _totalCrashes;
            set { _totalCrashes = value; OnPropertyChanged(nameof(TotalCrashes)); }
        }

        public TimeSpan TotalUptime
        {
            get => _totalUptime;
            set { _totalUptime = value; OnPropertyChanged(nameof(TotalUptime)); }
        }

        public List<ScheduleTask> Schedules { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}