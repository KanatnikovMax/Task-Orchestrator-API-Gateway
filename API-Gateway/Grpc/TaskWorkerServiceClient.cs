using API_Gateway.Grpc.Interfaces;
using Grpc.Core;
using WorkersService.Grpc;
using GrpcTaskWorkersService = WorkersService.Grpc.TaskWorkerService.TaskWorkerServiceClient;

namespace API_Gateway.Grpc;

public class TaskWorkerService(GrpcTaskWorkersService grpcClient) : ITaskWorkerService
{
    public async Task ProcessTaskAsync(string taskId, CancellationToken cancellationToken)
    {
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);

            await grpcClient.ProcessTaskAsync(
                new ProcessTaskRequest { TaskId = taskId },
                deadline: deadline,
                cancellationToken: cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            throw new ApplicationException("Worker service unavailable", ex);
        }
        catch (RpcException ex)
        {
            throw new ApplicationException($"gRPC call failed: {ex.Status.Detail}", ex);
        }
    }
}