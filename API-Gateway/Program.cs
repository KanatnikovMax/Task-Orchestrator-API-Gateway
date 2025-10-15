using API_Gateway.IoC;
using API_Gateway.Realtime.Hubs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

builder.Services.ConfigureOpenApi();

builder.Services.AddControllers();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHub<TaskProgressHub>("/hubs/task-progress");

app.Run();