using Quartz;

namespace TeamRotator.Infrastructure.Jobs;

public abstract class BaseJob : IJob
{
    private readonly ILogger<BaseJob> _logger;

    protected BaseJob(ILogger<BaseJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting job execution: {JobType}", GetType().Name);
            await ExecuteJob(context);
            _logger.LogInformation("Job execution completed: {JobType}", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing job: {JobType}", GetType().Name);
            throw;
        }
    }

    protected abstract Task ExecuteJob(IJobExecutionContext context);
} 