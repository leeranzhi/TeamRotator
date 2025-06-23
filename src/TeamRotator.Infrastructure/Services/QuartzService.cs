using Microsoft.Extensions.Hosting;
using Quartz;
using TeamRotator.Infrastructure.Jobs;

namespace TeamRotator.Infrastructure.Services;

public class QuartzService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzService> _logger;
    private IScheduler? _scheduler;

    public QuartzService(
        ISchedulerFactory schedulerFactory,
        ILogger<QuartzService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            var assignmentUpdateJob = JobBuilder.Create<AssignmentUpdateJob>()
                .WithIdentity("AssignmentUpdateJob", "TeamRotator")
                .Build();

            var assignmentUpdateTrigger = TriggerBuilder.Create()
                .WithIdentity("AssignmentUpdateTrigger", "TeamRotator")
                .WithCronSchedule("0 0 0 * * ?") // Every day at midnight
                .Build();

            var slackJob = JobBuilder.Create<SendToSlackJob>()
                .WithIdentity("SendToSlackJob", "TeamRotator")
                .Build();

            var slackTrigger = TriggerBuilder.Create()
                .WithIdentity("SendToSlackTrigger", "TeamRotator")
                .WithCronSchedule("0 0 10 * * ?") // Every day at 10:00 AM
                .Build();

            await _scheduler.ScheduleJob(assignmentUpdateJob, assignmentUpdateTrigger, cancellationToken);
            await _scheduler.ScheduleJob(slackJob, slackTrigger, cancellationToken);

            _logger.LogInformation("Successfully scheduled all jobs");
            await _scheduler.Start(cancellationToken);
            _logger.LogInformation("Quartz scheduler started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Quartz scheduler");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_scheduler != null && !_scheduler.IsShutdown)
        {
            _logger.LogInformation("Shutting down Quartz scheduler");
            await _scheduler.Shutdown(cancellationToken);
            _logger.LogInformation("Quartz scheduler shutdown completed");
        }
    }
} 