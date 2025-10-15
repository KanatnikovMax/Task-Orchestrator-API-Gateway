namespace API_Gateway.Realtime.ClientInterfaces;

public interface ITaskProgressClient
{
    Task UpdateProgress(string taskId, int progress); 
}