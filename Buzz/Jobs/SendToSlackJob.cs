using Buzz.Model;
using Buzz.Services;
using Quartz;
using Serilog.Context;
using Task = System.Threading.Tasks.Task;

namespace Buzz.Jobs;

public class SendToSlackJob : IJob
{
    private readonly SendToSlackService _slackService;
    private readonly IWorkingDayCheckService _workingDayCheckService;
    private readonly ILogger<SendToSlackJob> _logger;

    public SendToSlackJob(SendToSlackService slackService, IWorkingDayCheckService workingDayCheckService, ILogger<SendToSlackJob> logger)
    {
        _slackService = slackService;
        _workingDayCheckService = workingDayCheckService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        _logger.LogInformation("Executing SendToSlackJob...");

        if (!await _workingDayCheckService.IsWorkingDayCheck(DateTime.Today))
        {
            _logger.LogInformation("Today is not a working day. Skipping sending Slack message.");
            return;
        }

        _logger.LogInformation("Sending Slack message...");
        try
        {
            await _slackService.SendSlackMessage();

            _logger.LogInformation("Successfully sent message to Slack.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message to Slack.");
            throw;
        }
    }
}