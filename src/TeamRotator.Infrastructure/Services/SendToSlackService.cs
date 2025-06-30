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
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendToSlackService> _logger;

    public SendToSlackService(
        IDbContextFactory<RotationDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SendToSlackService> logger)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSlackMessage()
    {
        try
        {
            var webhookUrl = _configuration["Slack:WebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogWarning("Slack webhook URL is not configured");
                return;
            }

            using var context = _contextFactory.CreateDbContext();
            var assignments = await context.TaskAssignments
                .Include(ta => ta.Task)
                .Include(ta => ta.Member)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (!assignments.Any())
            {
                _logger.LogInformation("No assignments found to send to Slack");
                return;
            }

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Current Task Assignments:");
            messageBuilder.AppendLine();

            foreach (var assignment in assignments)
            {
                messageBuilder.AppendLine($"• {assignment.Task?.TaskName}: <@{assignment.Member?.SlackId}> ({assignment.Member?.Host})");
                messageBuilder.AppendLine($"  Period: {assignment.StartDate:yyyy-MM-dd} to {assignment.EndDate:yyyy-MM-dd}");
            }

            var message = new { text = messageBuilder.ToString() };
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                webhookUrl,
                new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json"));

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
        }
    }

    public async Task SendFailedMessageToSlack(string failedMessage)
    {
        try
        {
            _logger.LogInformation("Sending failure message to Slack...");

            var payload = new { text = failedMessage };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClientFactory.CreateClient().PostAsync(_configuration["Slack:PersonalWebhookUrl"], content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Failure message sent to personal Slack URL!");
            }
            else
            {
                _logger.LogError($"Failed to send failure message to personal Slack URL. Status code: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"An error occurred while sending failure message to personal Slack URL: {e.Message}");
        }
    }
} 