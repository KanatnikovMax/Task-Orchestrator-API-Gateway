using Microsoft.OpenApi.Models;

namespace API_Gateway.IoC;

internal static class SwaggerConfigurator
{
    internal static void ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task-Orchestrator-API-Gateway", Version = "v1" });
        });
    }
}