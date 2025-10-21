namespace API_Gateway.Services.Interfaces;

public interface ITaskProgressService
{
    ValueTask UpdateProgressAsync(string taskId, int progress);
    ValueTask<int?> GetProgressAsync(string taskId);
}