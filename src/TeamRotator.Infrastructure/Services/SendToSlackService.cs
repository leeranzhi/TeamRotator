using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Infrastructure.Services;

public class SendToSlackService
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _slackWebhookUrl;
    private readonly string _personalSlackUrl;
    private readonly ILogger<SendToSlackService> _logger;

    public SendToSlackService(
        IDbContextFactory<RotationDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SendToSlackService> logger)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _slackWebhookUrl = configuration["Slack:WebhookUrl"] ?? throw new InvalidOperationException("Slack webhook URL not configured");
        _personalSlackUrl = configuration["Slack:PersonalWebhookUrl"] ?? string.Empty;
        _logger = logger;
    }

    public async Task SendSlackMessage()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            _logger.LogInformation("Sending Slack message...");

            var taskAssignments = await context.TaskAssignments
                .Join(context.Members,
                    taskAssignment => taskAssignment.MemberId,
                    member => member.Id,
                    (taskAssignment, member) => new
                    {
                        TaskName = context.Tasks
                            .Where(task => task.Id == taskAssignment.TaskId)
                            .Select(task => task.TaskName)
                            .FirstOrDefault(),
                        SlackId = member.SlackId,
                        MemberId = member.Id,
                        TaskAssignmentId = taskAssignment.Id
                    })
                .OrderBy(x => x.TaskAssignmentId)
                .ToListAsync();

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Today's task assignments:");
            
            foreach (var assignment in taskAssignments)
            {
                if (!string.IsNullOrEmpty(assignment.TaskName))
                {
                    messageBuilder.AppendLine($"• {assignment.TaskName}: <@{assignment.SlackId}>");
                }
            }

            var message = new { text = messageBuilder.ToString() };
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(_slackWebhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send message to Slack. Status code: {response.StatusCode}");
            }

            _logger.LogInformation("Successfully sent Slack message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            await SendFailedMessageToSlack(ex.Message);
            throw;
        }
    }

    public async Task SendFailedMessageToSlack(string errorMessage)
    {
        if (string.IsNullOrEmpty(_personalSlackUrl))
        {
            _logger.LogWarning("Personal Slack webhook URL not configured. Cannot send failure notification.");
            return;
        }

        try
        {
            var message = new { text = $"Error in TeamRotator: {errorMessage}" };
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            await client.PostAsync(_personalSlackUrl, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error message to personal Slack");
        }
    }
} 