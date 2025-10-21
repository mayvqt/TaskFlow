using Microsoft.Win32;
using System.IO;
using System.Windows;
using TaskFlow.Models;

namespace TaskFlow.Views
{
    public partial class EditApplicationDialog : Window
    {
        public MonitoredApplication? Application { get; private set; }

        public EditApplicationDialog(MonitoredApplication application)
        {
            InitializeComponent();
            Application = application;
            LoadApplicationData();
        }

        private void LoadApplicationData()
        {
            if (Application != null)
            {
                NameTextBox.Text = Application.Name;
                ExecutablePathTextBox.Text = Application.ExecutablePath;
                ArgumentsTextBox.Text = Application.Arguments;
                WorkingDirectoryTextBox.Text = Application.WorkingDirectory;
                IsEnabledCheckBox.IsChecked = Application.IsEnabled;
                RestartOnCrashCheckBox.IsChecked = Application.RestartOnCrash;
                MaxRestartAttemptsTextBox.Text = Application.MaxRestartAttempts.ToString();
                StartupDelayTextBox.Text = ((int)Application.StartupDelay.TotalSeconds).ToString();
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|Batch files (*.bat)|*.bat|All files (*.*)|*.*",
                Title = "Select Application Executable"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExecutablePathTextBox.Text = openFileDialog.FileName;
                
                // Auto-fill working directory if empty
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            if (Application != null)
            {
                Application.Name = NameTextBox.Text.Trim();
                Application.ExecutablePath = ExecutablePathTextBox.Text.Trim();
                Application.Arguments = ArgumentsTextBox.Text.Trim();
                Application.WorkingDirectory = WorkingDirectoryTextBox.Text.Trim();
                Application.IsEnabled = IsEnabledCheckBox.IsChecked ?? true;
                Application.RestartOnCrash = RestartOnCrashCheckBox.IsChecked ?? true;
                Application.MaxRestartAttempts = int.Parse(MaxRestartAttemptsTextBox.Text);
                Application.StartupDelay = TimeSpan.FromSeconds(int.Parse(StartupDelayTextBox.Text));
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter an application name.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ExecutablePathTextBox.Text))
            {
                MessageBox.Show("Please select an executable file.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecutablePathTextBox.Focus();
                return false;
            }

            if (!File.Exists(ExecutablePathTextBox.Text))
            {
                MessageBox.Show("The selected executable file does not exist.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecutablePathTextBox.Focus();
                return false;
            }

            if (!int.TryParse(StartupDelayTextBox.Text, out int startupDelaySeconds) || startupDelaySeconds < 0)
            {
                MessageBox.Show("Please enter a valid startup delay (0 or greater).", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                StartupDelayTextBox.Focus();
                return false;
            }

            if (!int.TryParse(MaxRestartAttemptsTextBox.Text, out int maxRestartAttempts) || maxRestartAttempts < 0)
            {
                MessageBox.Show("Please enter a valid number of max restart attempts (0 or greater).", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                MaxRestartAttemptsTextBox.Focus();
                return false;
            }

            return true;
        }
    }
}