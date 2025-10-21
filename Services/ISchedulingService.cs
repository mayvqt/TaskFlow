using TaskFlow.Models;

namespace TaskFlow.Services
{
    public interface ISchedulingService
    {
        event EventHandler<ScheduleTask>? TaskExecuted;
        Task StartSchedulingAsync();
        Task StopSchedulingAsync();
        Task AddScheduleAsync(ScheduleTask schedule);
        Task RemoveScheduleAsync(string scheduleId);
        Task UpdateScheduleAsync(ScheduleTask schedule);
        Task ExecuteStartupTasksAsync();
        List<ScheduleTask> GetPendingTasks();
    }
}