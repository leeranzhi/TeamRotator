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
    private readonly HttpClient _httpClient;
    private readonly string _slackWebhookUrl;
    private readonly string _personalSlackUrl;
    private readonly ILogger<SendToSlackService> _logger;

    public SendToSlackService(
        IDbContextFactory<RotationDbContext> contextFactory,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SendToSlackService> logger)
    {
        _contextFactory = contextFactory;
        _httpClient = httpClient;
        _logger = logger;
        _slackWebhookUrl = configuration["Slack:WebhookUrl"] ?? throw new InvalidOperationException("Slack webhook URL not configured");
        _personalSlackUrl = configuration["Slack:PersonalWebhookUrl"] ?? throw new InvalidOperationException("Personal Slack webhook URL not configured");
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

            var members = await context.Members
                .OrderBy(member => member.Id)
                .ToListAsync();

            var messageBuilder = new StringBuilder();

            foreach (var assignment in taskAssignments)
            {
                messageBuilder.AppendLine($"{assignment.TaskName}: <@{assignment.SlackId}>");

                if (assignment.TaskName == "English word")
                {
                    var currentMemberIndex = members.FindIndex(m => m.Id == assignment.MemberId);
                    var nextOneMember = members[(currentMemberIndex + 1) % members.Count];
                    var nextTwoMember = members[(currentMemberIndex + 2) % members.Count];

                    messageBuilder.AppendLine($"English word(Day + 1): <@{nextOneMember.SlackId}>");
                    messageBuilder.AppendLine($"English word(Day + 2): <@{nextTwoMember.SlackId}>");
                }
            }

            var payload = new { text = messageBuilder.ToString() };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_slackWebhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Message sent successfully to Slack!");
            }
            else
            {
                _logger.LogError($"Failed to send message to Slack. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while sending message to Slack: {ex.Message}");
        }
    }

    public async Task SendFailedMessageToSlack(string failedMessage)
    {
        try
        {
            _logger.LogInformation("Sending failure message to Slack...");

            var payload = new { text = failedMessage };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_personalSlackUrl, content);

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