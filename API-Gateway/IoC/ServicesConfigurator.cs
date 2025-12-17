using API_Gateway.Options;
using API_Gateway.Services;
using API_Gateway.Services.Interfaces;

namespace API_Gateway.IoC;

internal static class ServicesConfigurator
{
    public static IServiceCollection AddTaskProgressService(this IServiceCollection services)
    {
        services.AddSingleton<ITaskProgressService, TaskProgressService>();
        
        return services;
    }

    public static WebApplicationBuilder AddKafkaProducer(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<KafkaOptions>(
            builder.Configuration.GetSection("Kafka"));
        builder.Services.AddSingleton<ITasksProducer, TasksProducer>();
        
        return builder;
    }
}