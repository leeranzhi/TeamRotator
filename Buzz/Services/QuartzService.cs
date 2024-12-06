using Quartz;
using Buzz.Jobs;
using Buzz.Model;
using Task = System.Threading.Tasks.Task;

namespace Buzz.Services;

public class QuartzService
{
    private readonly IScheduler _scheduler;
    private readonly IWorkingDayCheckService _workingDayCheckService;

    public QuartzService(IScheduler scheduler, IWorkingDayCheckService workingDayCheckService)
    {
        _scheduler = scheduler;
        _workingDayCheckService = workingDayCheckService;
    }

    public async Task ConfigureJobsAsync()
    {
        DateTime currentDate = DateTime.Today;
        bool isWorkingDay = await _workingDayCheckService.IsWorkingDay(currentDate);

        if (isWorkingDay)
        {
            var assignmentJobKey = new JobKey("AssignmentUpdateJob");
            var assignmentJobDetail = JobBuilder.Create<AssignmentUpdateJob>()
                .WithIdentity(assignmentJobKey)
                .Build();

            var assignmentJobTrigger = TriggerBuilder.Create()
                .WithIdentity("AssignmentUpdateJob-trigger")
                .WithCronSchedule("0 0 0 * * ?")
                .Build();

            await _scheduler.ScheduleJob(assignmentJobDetail, assignmentJobTrigger);

            var slackJobKey = new JobKey("SendToSlackJob");
            var slackJobDetail = JobBuilder.Create<SendToSlackJob>()
                .WithIdentity(slackJobKey)
                .Build();

            var slackJobTrigger = TriggerBuilder.Create()
                .WithIdentity("SendToSlackJob-trigger")
                .WithCronSchedule("0 0 8 * * ?")
                .Build();

            await _scheduler.ScheduleJob(slackJobDetail, slackJobTrigger);
        }
    }
}