using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Infrastructure.Services;

public class SendToSlackService
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SendToSlackService> _logger;
    private const string WEBHOOK_URL_KEY = "Slack:WebhookUrl";

    public SendToSlackService(
        IDbContextFactory<RotationDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<SendToSlackService> logger)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendSlackMessage()
    {
        var message = await GetSlackMessage();
        if (message == null)
        {
            return;
        }

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var webhookConfig = await context.SystemConfigs
                .FirstOrDefaultAsync(c => c.Key == WEBHOOK_URL_KEY);

            var webhookUrl = webhookConfig?.Value;
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogWarning("Slack webhook URL is not configured in the database");
                return;
            }

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                webhookUrl,
                new StringContent(JsonSerializer.Serialize(new { text = message }), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send Slack message. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, error);
                return;
            }

            _logger.LogInformation("Successfully sent Slack message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            throw;
        }
    }

    public async Task<string?> GetSlackMessage()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var taskAssignments = await context.TaskAssignments
                .Include(ta => ta.Task)
                .Include(ta => ta.Member)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (!taskAssignments.Any())
            {
                _logger.LogInformation("No assignments found to send to Slack");
                return null;
            }

            var members = await context.Members
                .OrderBy(m => m.Id)
                .ToListAsync();

            var messageBuilder = new StringBuilder();

            foreach (var assignment in taskAssignments)
            {
                messageBuilder.AppendLine($"{assignment.Task?.TaskName}: <@{assignment.Member?.SlackId}>");

                // Special handling for English word task
                if (assignment.Task?.TaskName == "English word")
                {
                    var currentMemberIndex = members.FindIndex(m => m.Id == assignment.MemberId);
                    var nextOneMember = members[(currentMemberIndex + 1) % members.Count];
                    var nextTwoMember = members[(currentMemberIndex + 2) % members.Count];

                    messageBuilder.AppendLine($"English word(Day + 1): <@{nextOneMember.SlackId}>");
                    messageBuilder.AppendLine($"English word(Day + 2): <@{nextTwoMember.SlackId}>");
                }
            }

            return messageBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Slack message");
            throw;
        }
    }

    public async Task SendFailedMessageToSlack(string failedMessage)
    {
        try
        {
            _logger.LogInformation("Sending failure message to Slack...");

            using var context = _contextFactory.CreateDbContext();
            var webhookConfig = await context.SystemConfigs
                .FirstOrDefaultAsync(c => c.Key == WEBHOOK_URL_KEY);

            var webhookUrl = webhookConfig?.Value;
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogWarning("Slack webhook URL is not configured in the database");
                return;
            }

            var payload = new { text = failedMessage };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClientFactory.CreateClient().PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Failure message sent to Slack!");
            }
            else
            {
                _logger.LogError($"Failed to send failure message to Slack. Status code: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"An error occurred while sending failure message to Slack: {e.Message}");
        }
    }
} 