using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Buzz.Services;

public class SendToSlackService(IDbContextFactory<RotationDbContext> contextFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AssignmentUpdateService> logger)
{
    private readonly string _slackWebhookUrl = configuration["Slack:WebhookUrl"];
    private readonly string _personalSlackUrl = configuration["Slack:PersonalWebhookUrl"];

    public async Task SendSlackMessage()
    {
        try
        {
            using var context = contextFactory.CreateDbContext();

            logger.LogInformation("Sending Slack message...");

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

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync(_slackWebhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Message sent successfully to Slack!");
            }
            else
            {
                logger.LogError($"Failed to send message to Slack. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while sending message to Slack: {ex.Message}");
        }
    }

    public async Task SendFailedMessageToSlack(string failedMessage)
    {
        try
        {
            logger.LogInformation("Sending failure message to Slack...");

            var payload = new { text = failedMessage };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync(_personalSlackUrl, content);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Failure message sent to personal Slack URL!");
            }
            else
            {
                logger.LogError($"Failed to send failure message to personal Slack URL. Status code: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            logger.LogError($"An error occurred while sending failure message to personal Slack URL: {e.Message}");
        }
    }
}
