using Buzz.Jobs;
using Buzz.Model;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace Buzz.Services;

public class QuartzService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IWorkingDayCheckService _workingDayCheckService;
    private readonly ILogger<QuartzService> _logger;

    public QuartzService(ISchedulerFactory schedulerFactory, IWorkingDayCheckService workingDayCheckService, ILogger<QuartzService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _workingDayCheckService = workingDayCheckService;
        _logger = logger;
    }

    public async Task ConfigureJobsAsync()
    {
        var scheduler = await _schedulerFactory.GetScheduler();

        var assignmentJobKey = new JobKey("AssignmentUpdateJob");
        var assignmentJobDetail = JobBuilder.Create<AssignmentUpdateJob>()
            .WithIdentity(assignmentJobKey)
            .Build();

        var assignmentJobTrigger = TriggerBuilder.Create()
            .WithIdentity("AssignmentUpdateJob-trigger")
            .WithCronSchedule("0 0 0 * * ?")
            .Build();

        await scheduler.ScheduleJob(assignmentJobDetail, assignmentJobTrigger);

        var slackJobKey = new JobKey("SendToSlackJob");
        var slackJobDetail = JobBuilder.Create<SendToSlackJob>()
            .WithIdentity(slackJobKey)
            .Build();

        var slackJobTrigger = TriggerBuilder.Create()
            .WithIdentity("SendToSlackJob-trigger")
            .WithCronSchedule("0 0 8 * * ?")
            .Build();
        
        await scheduler.ScheduleJob(slackJobDetail, slackJobTrigger);
        
        _logger.LogInformation("Jobs configured successfully. They will only execute on working days.");
    }
}
