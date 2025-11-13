using API_Gateway.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace API_Gateway.Services;

public class TaskProgressService : ITaskProgressService
{
    private static readonly TimeSpan CacheAbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CacheSlidingExpiration = TimeSpan.FromSeconds(30);

    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly ILogger<TaskProgressService> _logger;

    public TaskProgressService(IMemoryCache memoryCache, ILogger<TaskProgressService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheAbsoluteExpirationRelativeToNow,
            SlidingExpiration = CacheSlidingExpiration,
        };
    }

    public ValueTask UpdateProgressAsync(string taskId, int progress)
    {
        try
        {
            _memoryCache.Set($"task_progress_{taskId}", progress, _cacheOptions);
            
            _logger.LogDebug("Updated progress for task {TaskId}: {Progress}%", 
                taskId, progress);
            
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update progress for task {TaskId}", taskId);
            return ValueTask.FromException(ex);
        }
    }

    public ValueTask<int?> GetProgressAsync(string taskId)
    {
        try
        {
            var cacheKey = $"task_progress_{taskId}";
            return _memoryCache.TryGetValue(cacheKey, out int? progress) 
                ? ValueTask.FromResult(progress) 
                : ValueTask.FromResult<int?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get progress for task {TaskId}", taskId);
            return ValueTask.FromException<int?>(ex);
        }
    }
}