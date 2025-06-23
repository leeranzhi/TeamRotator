using Quartz;
using TeamRotator.Infrastructure.Services;

namespace TeamRotator.Infrastructure.Jobs;

public class SendToSlackJob : BaseJob
{
    private readonly SendToSlackService _slackService;

    public SendToSlackJob(
        ILogger<SendToSlackJob> logger,
        SendToSlackService slackService)
        : base(logger)
    {
        _slackService = slackService;
    }

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        await _slackService.SendSlackMessage();
    }
} 