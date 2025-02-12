using Buzz.Services;
using Quartz;
using Serilog.Context;

namespace Buzz.Jobs;

public class SendToSlackJob : IJob
{
    private readonly SendToSlackService _sendToSlackService;
    private readonly ILogger<SendToSlackJob> _logger;

    public SendToSlackJob(SendToSlackService sendToSlackService, ILogger<SendToSlackJob> logger)
    {
        _sendToSlackService = sendToSlackService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        _logger.LogInformation("Executing SendToSlackJob...");

        try
        {
            await _sendToSlackService.SendSlackMessage();

            _logger.LogInformation("Successfully sent message to Slack.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message to Slack.");
            throw;
        }
    }
}