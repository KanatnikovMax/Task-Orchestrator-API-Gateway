namespace API_Gateway.Models;

public class KafkaTaskRequest
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}