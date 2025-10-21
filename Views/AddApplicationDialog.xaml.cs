using Microsoft.Win32;
using System.IO;
using System.Windows;
using TaskFlow.Models;

namespace TaskFlow.Views
{
    public partial class AddApplicationDialog : Window
    {
        public MonitoredApplication? Application { get; private set; }

        public AddApplicationDialog()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Application Executable",
                Filter = "Executable Files (*.exe)|*.exe|Batch Files (*.bat)|*.bat|All Files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExecutablePathTextBox.Text = openFileDialog.FileName;
                
                // Auto-populate name if empty
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }

                // Auto-populate working directory if empty
                if (string.IsNullOrWhiteSpace(WorkingDirectoryTextBox.Text))
                {
                    var directory = Path.GetDirectoryName(openFileDialog.FileName);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        WorkingDirectoryTextBox.Text = directory;
                    }
                }
            }
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, just allow manual entry - we can implement a folder browser later
            MessageBox.Show("Please manually enter the working directory path for now.", 
                "Folder Browser", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter an application name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ExecutablePathTextBox.Text))
            {
                MessageBox.Show("Please select an executable file.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecutablePathTextBox.Focus();
                return;
            }

            if (!File.Exists(ExecutablePathTextBox.Text))
            {
                MessageBox.Show("The selected executable file does not exist.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecutablePathTextBox.Focus();
                return;
            }

            // Parse startup delay
            if (!int.TryParse(StartupDelayTextBox.Text, out int startupDelaySeconds) || startupDelaySeconds < 0)
            {
                MessageBox.Show("Please enter a valid startup delay (0 or positive number).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                StartupDelayTextBox.Focus();
                return;
            }

            // Parse max restart attempts
            if (!int.TryParse(MaxRestartAttemptsTextBox.Text, out int maxRestartAttempts) || maxRestartAttempts < 0)
            {
                MessageBox.Show("Please enter a valid number of restart attempts (0 or positive number).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MaxRestartAttemptsTextBox.Focus();
                return;
            }

            // Create the application
            Application = new MonitoredApplication
            {
                Name = NameTextBox.Text.Trim(),
                ExecutablePath = ExecutablePathTextBox.Text.Trim(),
                Arguments = ArgumentsTextBox.Text.Trim(),
                WorkingDirectory = WorkingDirectoryTextBox.Text.Trim(),
                IsEnabled = IsEnabledCheckBox.IsChecked ?? true,
                RestartOnCrash = RestartOnCrashCheckBox.IsChecked ?? true,
                MaxRestartAttempts = maxRestartAttempts,
                StartupDelay = TimeSpan.FromSeconds(startupDelaySeconds)
            };

            // Add startup schedule if requested
            if (AutoStartCheckBox.IsChecked == true)
            {
                var startupSchedule = new ScheduleTask
                {
                    ApplicationId = Application.Id,
                    Name = $"Auto-start {Application.Name}",
                    ScheduleType = ScheduleType.Startup,
                    Action = TaskAction.Start,
                    IsEnabled = true
                };
                Application.Schedules.Add(startupSchedule);
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}