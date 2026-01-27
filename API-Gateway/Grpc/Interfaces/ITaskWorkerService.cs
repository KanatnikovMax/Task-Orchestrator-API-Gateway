namespace API_Gateway.Grpc.Interfaces;

public interface ITaskWorkerService
{
    Task ProcessTaskAsync(string taskId, CancellationToken cancellationToken);
}