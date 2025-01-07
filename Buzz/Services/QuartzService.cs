using Buzz.Jobs;
using Buzz.Model;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace Buzz.Services;

public class QuartzService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IWorkingDayCheckService _workingDayCheckService;

    public QuartzService(ISchedulerFactory schedulerFactory, IWorkingDayCheckService workingDayCheckService)
    {
        _schedulerFactory = schedulerFactory;
        _workingDayCheckService = workingDayCheckService;
    }

    public async Task ConfigureJobsAsync()
    {
        DateTime currentDate = DateTime.Today;
        bool isWorkingDay = await _workingDayCheckService.IsWorkingDayCheck(currentDate);

        if (isWorkingDay)
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
        }
    }
}
