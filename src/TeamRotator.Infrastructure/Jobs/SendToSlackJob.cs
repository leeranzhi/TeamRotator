using Microsoft.Extensions.Logging;
using Quartz;
using TeamRotator.Infrastructure.Services;

namespace TeamRotator.Infrastructure.Jobs;

public class SendToSlackJob : BaseJob
{
    private readonly SendToSlackService _sendToSlackService;

    public SendToSlackJob(
        SendToSlackService sendToSlackService,
        ILogger<SendToSlackJob> logger) : base(logger)
    {
        _sendToSlackService = sendToSlackService;
    }

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting SendToSlackJob at: {time}", DateTimeOffset.Now);
        try
        {
            await _sendToSlackService.SendSlackMessage();
            _logger.LogInformation("SendToSlackJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing SendToSlackJob");
            throw;
        }
    }
} 