using API_Gateway.Realtime.ClientInterfaces;
using API_Gateway.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API_Gateway.Realtime.Hubs;

internal sealed class TaskProgressHub(
    ITaskProgressService taskProgressService,
    ILogger<TaskProgressHub> logger) : Hub<ITaskProgressClient>
{
    // Workers Service Method
    public async Task UpdateWorkerTaskProgress(string taskId, int progress)
    {
        logger.LogInformation($"UpdateWorkerTaskProgress: {taskId}, {progress}%");
        await taskProgressService.UpdateProgressAsync(taskId, progress);
        await Clients.Group($"task-{taskId}").UpdateProgress(taskId, progress);
    }

    // Frontend Methods
    public async Task SubscribeToTask(string taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"task-{taskId}");
        
        var currentProgress = await taskProgressService.GetProgressAsync(taskId) ?? 0;
        await Clients.Caller.UpdateProgress(taskId, currentProgress);
        
        logger.LogInformation($"SubscribeToTask {taskId}");
    }
    
    public async Task UnsubscribeFromTask(string taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task-{taskId}");
        
        logger.LogInformation($"UnsubscribeFromTask {taskId}");
    }
}