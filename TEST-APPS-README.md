# TaskFlow Test Applications

This folder contains several test batch files that you can use to test TaskFlow's application monitoring capabilities:

## Test Files

### 1. `test-app.bat`
- **Purpose**: Basic looping application 
- **Behavior**: Runs indefinitely, prints timestamp every 5 seconds
- **Use**: Good for testing basic start/stop functionality

### 2. `test-service.bat` 
- **Purpose**: Simulates a service application
- **Behavior**: Runs indefinitely with heartbeat every 10 seconds
- **Use**: Best for testing long-running service monitoring

### 3. `quick-test.bat` 
- **Purpose**: Short-duration test application
- **Behavior**: Runs for 60 seconds then exits automatically
- **Use**: Good for testing auto-restart and crash detection

## How to Use

1. Open TaskFlow application
2. Click "Add Application" 
3. Browse and select one of these .bat files
4. Configure monitoring options as needed
5. Test start/stop/restart functionality

## Testing Scenarios

- **Start/Stop**: Use any of the test apps
- **Auto-restart**: Use `quick-test.bat` with "Restart on Crash" enabled
- **Long-running monitoring**: Use `test-service.bat`
- **Multiple applications**: Add all three and monitor simultaneously