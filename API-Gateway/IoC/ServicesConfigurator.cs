using API_Gateway.Services;
using API_Gateway.Services.Interfaces;

namespace API_Gateway.IoC;

public static class ServicesConfigurator
{
    public static IServiceCollection AddTaskProgressService(this IServiceCollection services)
    {
        services.AddSingleton<ITaskProgressService, TaskProgressService>();
        
        return services;
    }
}