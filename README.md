# TaskFlow - Server Application Monitor

TaskFlow is a modern, sleek Windows Server application monitor built with .NET 8 and WPF. It provides comprehensive monitoring, management, and scheduling capabilities for server applications and services.

## 🚀 Current Status
- ✅ **Fully Functional** - Application builds and runs successfully
- ✅ **UI Working** - All buttons and interfaces are operational
- ✅ **Process Management** - Start, stop, restart applications
- ✅ **Real-time Monitoring** - Live status updates and monitoring
- ✅ **Modern Material Design UI** - Clean, professional interface

## Features

### 🔧 Application Management
- **Add/Remove Applications**: Easily manage your server applications and batch files
- **Real-time Status Monitoring**: Monitor application status with live updates every second
- **Process Control**: Start, stop, and restart applications with a single click
- **Crash Recovery**: Automatic restart on application crashes with configurable retry limits
- **Bulk Operations**: Start All / Stop All functionality for managing multiple applications
- **Application Validation**: Real-time process ID tracking and status verification

### ⏰ Advanced Scheduling
- **Startup Scheduling**: Configure applications to start automatically with configurable delays
- **Interval-based Tasks**: Run tasks at regular intervals (every X minutes/hours)
- **Daily Scheduling**: Schedule tasks at specific times each day
- **Weekly Scheduling**: Schedule tasks on specific days of the week
- **Smart Execution**: Automatic startup task execution when TaskFlow starts

### 🎨 Modern UI
- **Material Design**: Clean, modern interface using MaterialDesignInXaml
- **Dark Theme**: Professional dark theme perfect for server environments
- **Real-time Updates**: Live status updates and monitoring information
- **Tabbed Interface**: Organized tabs for Applications, Schedules, and Logs
- **Status Bar**: Real-time application count and system time display
- **Action Buttons**: Individual start/stop/restart/edit/delete buttons for each application

### 📊 Monitoring & Logging
- **System Information**: View system resources and performance metrics
- **Application Logs**: Centralized logging with configurable levels
- **Status Dashboard**: Real-time overview of running/stopped applications
- **Event Tracking**: Track application starts, stops, crashes, and restarts
- **Live Log View**: Real-time log display in the Logs tab

## Getting Started

### Prerequisites
- Windows Server 2019/2022 or Windows 10/11
- .NET 8.0 Runtime or SDK

### Installation

#### Option 1: Development (Recommended)
1. Clone or download the repository
2. Build the solution:
   ```powershell
   dotnet build TaskFlow.csproj
   ```
3. Run the application:
   ```powershell
   dotnet run
   ```
   Or use the provided batch file:
   ```powershell
   .\StartTaskFlow.bat
   ```

#### Option 2: Standalone Executable
1. Publish as standalone executable:
   ```powershell
   .\Publish.bat
   ```
2. Run the executable from the `publish` folder:
   ```powershell
   .\publish\TaskFlow.exe
   ```

### Quick Start Guide

1. **Launch TaskFlow** - Run using one of the methods above
2. **Add Your First Application**:
   - Click "Add Application" button
   - This will add a sample Notepad application for testing
   - For custom applications, you can edit the configuration file (see Configuration section)
3. **Test Process Management**:
   - Click the ▶️ (Start) button to launch the application
   - Click the ⏹️ (Stop) button to terminate it
   - Click the 🔄 (Restart) button to restart it
4. **Monitor in Real-time**: Watch the status updates in the main grid and logs tab

### Configuration
The application stores its configuration in:
`%APPDATA%\TaskFlow\config.json`

A sample configuration file (`sample-config.json`) is included in the project root.

You can also use the provided `sample-config.json` as a starting point.

## Usage

## Usage

### Managing Applications

#### Adding Applications
1. **Quick Test**: Click "Add Application" to add a sample Notepad application for testing
2. **Custom Applications**: 
   - Edit the configuration file manually, or
   - Use the planned dialog interface (coming soon)

#### Application Controls
- **▶️ Start**: Launch the selected application
- **⏹️ Stop**: Gracefully stop the application (5-second timeout, then force kill)
- **🔄 Restart**: Stop and restart the application with a 2-second delay
- **✏️ Edit**: Edit application settings (UI coming soon)
- **🗑️ Delete**: Remove application from monitoring

#### Bulk Operations
- **Start All**: Start all enabled applications
- **Stop All**: Stop all running applications
- **Refresh**: Reload the applications list

### Creating Schedules
1. Click "Add Schedule" in the Schedules tab
2. Select the target application
3. Configure the schedule:
   - **Schedule Type**: Startup, Interval, Daily, or Weekly
   - **Action**: Start, Stop, or Restart
   - **Time/Interval**: When to execute the task

### Monitoring Features
- **Real-time Status**: Application status updates every 5 seconds
- **Live Logs**: View application events in the Logs tab
- **Status Bar**: Shows running/stopped counts and current time
- **System Info**: Access via the ℹ️ button in the header

### Managing Applications
- **Start/Stop/Restart**: Use the action buttons in the Applications tab
- **Bulk Operations**: Use "Start All" or "Stop All" for multiple applications
- **Edit/Delete**: Use the edit and delete buttons for individual applications

## Architecture

TaskFlow is built using a clean, modular architecture following MVVM (Model-View-ViewModel) patterns:

### Technology Stack
- **.NET 8.0**: Latest .NET framework for optimal performance
- **WPF (Windows Presentation Foundation)**: Modern Windows desktop UI framework
- **Material Design In XAML**: Beautiful, consistent UI components
- **Microsoft.Extensions**: Dependency injection, logging, and hosting
- **Newtonsoft.Json**: Configuration serialization
- **System.ServiceProcess**: Windows service interaction

### Core Components

#### Services Layer
- **ProcessMonitorService**: Handles application lifecycle and real-time monitoring
- **SchedulingService**: Manages scheduled tasks and automatic execution
- **ConfigurationService**: Handles JSON-based configuration persistence
- **ApplicationManagementService**: Manages application CRUD operations and system info

#### Models
- **MonitoredApplication**: Represents a monitored application with status tracking
- **ScheduleTask**: Represents a scheduled task with timing and action definitions
- **AppConfiguration**: Application settings and configuration structure

#### ViewModels (MVVM Pattern)
- **MainWindowViewModel**: Main UI logic with command handling and data binding
- **ViewModelBase**: Base class providing INotifyPropertyChanged implementation
- **RelayCommand**: Command implementation for button actions

#### Views
- **MainWindow**: Material Design WPF interface with tabbed layout
- **Custom Controls**: Material Design components for modern UI

### Key Features Implementation

#### Real-time Monitoring
- Timer-based process checking every 5 seconds
- Automatic status updates via data binding
- Event-driven UI updates for responsive interface

#### Process Management
- Graceful shutdown with fallback to force termination
- Process ID tracking and validation
- Automatic restart capabilities with configurable retry limits

#### Dependency Injection
- Microsoft.Extensions.DependencyInjection for loose coupling
- Service lifetime management (Singleton/Transient)
- Clean separation of concerns

## Configuration Examples

### Application Configuration Structure
```json
{
  "Id": "unique-app-id",
  "Name": "My Web Server",
  "ExecutablePath": "C:\\inetpub\\wwwroot\\MyApp\\start.bat",
  "Arguments": "--port 8080 --env production",
  "WorkingDirectory": "C:\\inetpub\\wwwroot\\MyApp",
  "IsEnabled": true,
  "StartupDelay": "00:00:30",
  "RestartOnCrash": true,
  "MaxRestartAttempts": 3,
  "CurrentRestartAttempts": 0
}
```

### Schedule Configuration
```json
{
  "Id": "schedule-id",
  "ApplicationId": "unique-app-id",
  "Name": "Daily Restart at 3 AM",
  "ScheduleType": 3,
  "Action": 2,
  "Time": "03:00:00",
  "IsEnabled": true
}
```

### Complete Configuration File Example
See `sample-config.json` in the project root for a complete example with:
- Web server application with startup and daily restart schedules
- Database backup service with startup schedule
- Comprehensive application settings

### Schedule Types
- `0` = None
- `1` = Startup (runs when TaskFlow starts)
- `2` = Interval (runs every X time)
- `3` = Daily (runs at specific time each day)
- `4` = Weekly (runs on specific day/time each week)

### Action Types
- `0` = Start application
- `1` = Stop application
- `2` = Restart application

## Logging and Troubleshooting

### Log Levels
TaskFlow uses Microsoft.Extensions.Logging with these levels:
- **Information**: Normal application events and operations
- **Warning**: Recoverable issues, restart attempts, unexpected behavior
- **Error**: Application failures, startup errors, critical issues

### Log Locations
- **Real-time Logs**: View in the "Logs" tab within the application
- **Persistent Logs**: Stored in `%APPDATA%\TaskFlow\logs\` (when file logging is enabled)

### Common Issues and Solutions

#### Application Won't Start
1. Check if the executable path exists and is accessible
2. Verify the working directory is correct
3. Ensure the application has proper permissions
4. Check the Logs tab for detailed error messages

#### Process Monitoring Issues
1. Verify the application is not running as a different user
2. Check if antivirus software is blocking process access
3. Ensure TaskFlow is running with appropriate privileges

#### Configuration Problems
1. Validate JSON syntax in the configuration file
2. Check file permissions for `%APPDATA%\TaskFlow\config.json`
3. Use the provided `sample-config.json` as a reference

## Development and Contributing

### Building from Source
```powershell
# Clone the repository
git clone https://github.com/yourusername/TaskFlow.git
cd TaskFlow

# Restore packages and build
dotnet restore
dotnet build

# Run in development mode
dotnet run

# Create standalone executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Project Structure
```
TaskFlow/
├── Models/              # Data models and entities
├── Services/            # Business logic and data access
├── ViewModels/          # MVVM ViewModels
├── Views/               # WPF Views and UserControls
├── sample-config.json   # Example configuration
├── StartTaskFlow.bat    # Development launcher
└── Publish.bat         # Build script for release
```

TaskFlow uses Microsoft.Extensions.Logging for comprehensive logging:
- **Information**: General application events
- **Warning**: Recoverable issues and restart attempts
- **Error**: Application failures and exceptions

Log files are stored in the application data directory with automatic rotation.

## Screenshots

The application features a modern Material Design interface with:
- **Dark Theme**: Professional appearance suitable for server environments
- **Tabbed Layout**: Applications, Schedules, and Logs organized in separate tabs
- **Real-time Status**: Color-coded status indicators (Green=Running, Red=Stopped, Orange=Error)
- **Action Buttons**: Individual controls for each application
- **Status Bar**: Live counters and system time
- **Responsive Design**: Clean, organized layout that scales well

## Roadmap

### Planned Features
- [ ] **Application Add/Edit Dialog**: Rich UI for adding and editing applications
- [ ] **System Tray Integration**: Minimize to system tray functionality
- [ ] **Notifications**: Toast notifications for application events
- [ ] **Export/Import**: Configuration backup and restore
- [ ] **Performance Metrics**: CPU and memory usage monitoring
- [ ] **Web Interface**: Optional web-based management interface
- [ ] **Windows Service Mode**: Run TaskFlow as a Windows service
- [ ] **Multi-Server Support**: Manage applications across multiple servers

### Known Limitations
- Currently requires manual configuration file editing for custom applications
- No built-in application browser/selector
- Limited to Windows platform (.NET 8 WPF requirement)

## Support and Issues

### Getting Help
1. Check the **Logs tab** in the application for detailed error information
2. Review the **Configuration Examples** section for proper setup
3. Verify your system meets the **Prerequisites**
4. Check the **Common Issues** section for troubleshooting steps

### Reporting Issues
When reporting issues, please include:
- Windows version and .NET 8 runtime version
- TaskFlow version information
- Error messages from the Logs tab
- Configuration file content (remove sensitive information)
- Steps to reproduce the issue

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- **Material Design In XAML** for the beautiful UI components
- **Microsoft .NET Team** for the excellent .NET 8 framework
- **Community Contributors** for feedback and suggestions

---

*TaskFlow - Making Windows Server application management simple and reliable.*

## License

This project is licensed under the MIT License.