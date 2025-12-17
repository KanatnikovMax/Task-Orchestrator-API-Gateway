using API_Gateway.IoC;
using API_Gateway.Realtime.Hubs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

builder.Services.ConfigureOpenApi();

builder.AddKafkaProducer();

builder.Services.AddTaskProgressService();

builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
});

builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["Redis:Connection"]!, options =>
    {
        options.Configuration.ChannelPrefix = "SignalR";
    });

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<TaskProgressHub>("/hubs/task-progress");

app.MapGet("/status", () => Results.Ok(new { Environment.MachineName, }));

app.Run();