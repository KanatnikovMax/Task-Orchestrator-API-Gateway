using API_Gateway.Models;

namespace API_Gateway.Services.Interfaces;

public interface ITasksProducer
{
    Task<bool> ProduceTaskAsync(KafkaTaskRequest task, CancellationToken cancellationToken);
}