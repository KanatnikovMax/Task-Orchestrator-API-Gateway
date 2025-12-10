using API_Gateway.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace API_Gateway.Services;

public class TaskProgressService : ITaskProgressService
{
    private static readonly TimeSpan CacheAbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CacheSlidingExpiration = TimeSpan.FromSeconds(30);

    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _cacheOptions;
    private readonly ILogger<TaskProgressService> _logger;

    public TaskProgressService(IDistributedCache cache, ILogger<TaskProgressService> logger)
    {
        _cache = cache;
        _logger = logger;
        _cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheAbsoluteExpirationRelativeToNow,
            SlidingExpiration = CacheSlidingExpiration,
        };
    }

    public async ValueTask UpdateProgressAsync(string taskId, int progress)
    {
        try
        {
            var key = $"task_progress_{taskId}";
            var bytes = BitConverter.GetBytes(progress);
            await _cache.SetAsync(key, bytes, _cacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update progress for task {TaskId}", taskId);
        }
    }

    public async ValueTask<int?> GetProgressAsync(string taskId)
    {
        try
        {
            var key = $"task_progress_{taskId}";
            var bytes = await _cache.GetAsync(key);
            if (bytes == null)
            {
                return null;
            }
            
            return BitConverter.ToInt32(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get progress for task {TaskId}", taskId);
            return await ValueTask.FromException<int?>(ex);
        }
    }
}