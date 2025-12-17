namespace API_Gateway.Options;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = null!;
    public string Topic { get; set; } = null!;
}