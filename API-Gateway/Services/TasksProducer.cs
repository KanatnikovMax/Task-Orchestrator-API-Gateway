using System.Text.Json;
using API_Gateway.Models;
using API_Gateway.Options;
using API_Gateway.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace API_Gateway.Services;

public class TasksProducer : ITasksProducer
{
    private readonly ILogger<TasksProducer> _logger;
    private readonly string _topic;
    private readonly IProducer<Null, string> _producer;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public TasksProducer(IOptions<KafkaOptions> options, ILogger<TasksProducer> logger)
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        _logger = logger;
        _topic = options.Value.Topic;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
        };
        
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }
    
    public async Task<bool> ProduceTaskAsync(KafkaTaskRequest task, CancellationToken cancellationToken)
    {
        try
        {
            var message = new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(task, _jsonSerializerOptions)
            };

            var deliveryResult = await _producer.ProduceAsync(_topic, message, cancellationToken);
            
            _logger.LogInformation("Task {TaskId} produced to Kafka. Partition: {Partition}, Offset: {Offset}", 
                task.TaskId, deliveryResult.Partition, deliveryResult.Offset.Value);
            
            return deliveryResult.Status == PersistenceStatus.Persisted;
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Failed to produce task {TaskId} to Kafka", task.TaskId);
            return false;
        }
    }
}