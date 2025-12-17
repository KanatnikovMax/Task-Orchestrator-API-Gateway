using API_Gateway.Models;
using API_Gateway.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(
    ILogger<TasksController> logger, 
    ITasksProducer kafkaProducer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var task = new KafkaTaskRequest
        {
            TaskId = request.TaskName,
        };

        var success = await kafkaProducer.ProduceTaskAsync(task, cancellationToken);
        
        if (!success)
        {
            return StatusCode(500, new { error = "Failed to schedule task" });
        }

        logger.LogInformation("Task {TaskId} created and sent to Kafka", task.TaskId);

        return NoContent();
    }

    [HttpPost("{taskId}")]
    public async Task<IActionResult> ProcessTask(string taskId, CancellationToken cancellationToken)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://workers-service:5208");
        await client.GetAsync(taskId, cancellationToken);
        
        return Ok();
    }
}