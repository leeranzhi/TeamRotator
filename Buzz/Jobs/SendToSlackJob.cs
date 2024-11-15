using Buzz.Services;
using Quartz;

namespace Buzz.Jobs;

public class SendToSlackJob : IJob
{
    private readonly SendToSlackService _sendToSlackService;

    public SendToSlackJob(SendToSlackService sendToSlackService)
    {
        _sendToSlackService = sendToSlackService;
    }

    public System.Threading.Tasks.Task Execute(IJobExecutionContext context)
    {
        _sendToSlackService.SendSlackMessage();
        return System.Threading.Tasks.Task.CompletedTask;
    }
}