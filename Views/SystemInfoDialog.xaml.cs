using System.Reflection;
using System.Windows;
using TaskFlow.Services;

namespace TaskFlow.Views
{
    public partial class SystemInfoDialog : Window
    {
        private readonly IApplicationManagementService? _applicationService;
        private readonly DateTime _sessionStartTime;

        public SystemInfoDialog(IApplicationManagementService? applicationService = null)
        {
            InitializeComponent();
            _applicationService = applicationService;
            _sessionStartTime = DateTime.Now;
            LoadSystemInformation();
        }

        private async void LoadSystemInformation()
        {
            try
            {
                // Application Information
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                VersionTextBlock.Text = version?.ToString() ?? "1.0.0";
                
                var buildDate = GetBuildDate(assembly);
                BuildDateTextBlock.Text = buildDate.ToString("MMMM dd, yyyy");
                
                RuntimeTextBlock.Text = Environment.Version.ToString();
                PlatformTextBlock.Text = $"{Environment.OSVersion.Platform} ({Environment.ProcessorCount} cores)";

                // System Information
                OSVersionTextBlock.Text = Environment.OSVersion.ToString();
                MachineNameTextBlock.Text = Environment.MachineName;
                UserNameTextBlock.Text = Environment.UserName;
                WorkingDirTextBlock.Text = Environment.CurrentDirectory;

                // Session Statistics
                SessionStartTextBlock.Text = _sessionStartTime.ToString("MM/dd/yyyy HH:mm:ss");

                // Application statistics (if service is available)
                if (_applicationService != null)
                {
                    var applications = await _applicationService.GetApplicationsAsync();
                    TotalAppsTextBlock.Text = applications.Count().ToString();
                    
                    var runningApps = applications.Count(app => app.Status == Models.ApplicationStatus.Running);
                    RunningAppsTextBlock.Text = runningApps.ToString();
                    
                    var totalSchedules = applications.SelectMany(app => app.Schedules).Count();
                    TotalSchedulesTextBlock.Text = totalSchedules.ToString();
                }
                else
                {
                    TotalAppsTextBlock.Text = "N/A";
                    RunningAppsTextBlock.Text = "N/A";
                    TotalSchedulesTextBlock.Text = "N/A";
                }
            }
            catch (Exception ex)
            {
                // If there's an error loading stats, show basic info
                TotalAppsTextBlock.Text = "Error loading statistics";
                RunningAppsTextBlock.Text = ex.Message;
            }
        }

        private static DateTime GetBuildDate(Assembly assembly)
        {
            try
            {
                var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (attribute != null && DateTime.TryParse(attribute.InformationalVersion, out DateTime buildDate))
                {
                    return buildDate;
                }
            }
            catch
            {
                // Fallback to current date if we can't get build date
            }
            
            return DateTime.Today;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}