using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Buzz.Services;

public class SendToSlackService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly string _slackWebhookUrl;
    private readonly string _personalSlackUrl;

    public SendToSlackService(
        IDbContextFactory<RotationDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _slackWebhookUrl = configuration["Slack:WebhookUrl"];
        _personalSlackUrl = configuration["Slack:PersonalWebhookUrl"];
    }

    public async Task SendSlackMessage()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

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

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(_slackWebhookUrl, content);

            Console.WriteLine(response.IsSuccessStatusCode
                ? "Message sent successfully to Slack!"
                : $"Failed to send message to Slack. Status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending message to Slack: {ex.Message}");
        }
    }

    public async Task SendFailedMessageToSlack(string failedMessage)
    {
        try
        {
            var payload = new { text = failedMessage };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(_personalSlackUrl, content);

            Console.WriteLine(response.IsSuccessStatusCode
                ? "Failure message sent to personal Slack URL!"
                : $"Failed to send failure message to personal Slack URL. Status code: {response.StatusCode}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while sending failure message to personal Slack URL: {e.Message}");
        }
    }
}
