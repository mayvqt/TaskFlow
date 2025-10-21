using System.ComponentModel;

namespace TaskFlow.Models
{
    public enum TaskAction
    {
        Start,
        Stop,
        Restart
    }

    public class ScheduleTask : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private ScheduleType _scheduleType = ScheduleType.None;
        private TaskAction _action = TaskAction.Start;
        private TimeSpan _time = TimeSpan.Zero;
        private TimeSpan _interval = TimeSpan.FromMinutes(30);
        private DayOfWeek _dayOfWeek = DayOfWeek.Monday;
        private bool _isEnabled = true;
        private DateTime? _lastExecuted;
        private DateTime? _nextExecution;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ApplicationId { get; set; } = string.Empty;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public ScheduleType ScheduleType
        {
            get => _scheduleType;
            set { _scheduleType = value; OnPropertyChanged(nameof(ScheduleType)); CalculateNextExecution(); }
        }

        public TaskAction Action
        {
            get => _action;
            set { _action = value; OnPropertyChanged(nameof(Action)); }
        }

        public TimeSpan Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(nameof(Time)); CalculateNextExecution(); }
        }

        public TimeSpan Interval
        {
            get => _interval;
            set { _interval = value; OnPropertyChanged(nameof(Interval)); CalculateNextExecution(); }
        }

        public DayOfWeek DayOfWeek
        {
            get => _dayOfWeek;
            set { _dayOfWeek = value; OnPropertyChanged(nameof(DayOfWeek)); CalculateNextExecution(); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public DateTime? LastExecuted
        {
            get => _lastExecuted;
            set { _lastExecuted = value; OnPropertyChanged(nameof(LastExecuted)); CalculateNextExecution(); }
        }

        public DateTime? NextExecution
        {
            get => _nextExecution;
            set { _nextExecution = value; OnPropertyChanged(nameof(NextExecution)); }
        }

        private void CalculateNextExecution()
        {
            var now = DateTime.Now;
            
            switch (ScheduleType)
            {
                case ScheduleType.Startup:
                    NextExecution = null; // Executes only on startup
                    break;
                
                case ScheduleType.Interval:
                    if (LastExecuted.HasValue)
                        NextExecution = LastExecuted.Value.Add(Interval);
                    else
                        NextExecution = now.Add(Interval);
                    break;
                
                case ScheduleType.Daily:
                    var today = now.Date.Add(Time);
                    NextExecution = today > now ? today : today.AddDays(1);
                    break;
                
                case ScheduleType.Weekly:
                    var daysUntilTarget = ((int)DayOfWeek - (int)now.DayOfWeek + 7) % 7;
                    var targetDate = now.Date.AddDays(daysUntilTarget).Add(Time);
                    
                    if (targetDate <= now)
                        targetDate = targetDate.AddDays(7);
                    
                    NextExecution = targetDate;
                    break;
                
                default:
                    NextExecution = null;
                    break;
            }
            
            OnPropertyChanged(nameof(NextExecution));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}