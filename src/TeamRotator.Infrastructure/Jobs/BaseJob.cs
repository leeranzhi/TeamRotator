using Microsoft.Extensions.Logging;
using Quartz;

namespace TeamRotator.Infrastructure.Jobs;

public abstract class BaseJob : IJob
{
    protected readonly ILogger<BaseJob> _logger;

    protected BaseJob(ILogger<BaseJob> logger)
    {
        _logger = logger;
    }

    public virtual async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await ExecuteJob(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing job");
            throw;
        }
    }

    protected abstract Task ExecuteJob(IJobExecutionContext context);
} 