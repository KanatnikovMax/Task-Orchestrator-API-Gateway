using Serilog;

namespace API_Gateway.IoC;

internal static class SerilogConfigurator
{
    internal static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .Enrich.WithCorrelationId()
                .ReadFrom.Configuration(context.Configuration);
        });

        builder.Services.AddHttpContextAccessor();
    }
}