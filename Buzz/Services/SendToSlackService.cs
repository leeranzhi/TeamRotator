using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Buzz.Services
{
    public class SendToSlackService
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly IDbContextFactory<RotationDbContext> _contextFactory;

        public SendToSlackService(IDbContextFactory<RotationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task SendSlackMessage()
        {
            const string slackWebhookUrl = "";

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
                            TaskAssignmentId = taskAssignment.Id
                        })
                    .OrderBy(x => x.TaskAssignmentId)
                    .ToListAsync();

                var messageBuilder = new StringBuilder();
                foreach (var assignment in taskAssignments)
                {
                    messageBuilder.AppendLine($"{assignment.TaskName}: <@{assignment.SlackId}>");
                }

                var payload = new { text = messageBuilder.ToString() };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await Client.PostAsync(slackWebhookUrl, content);

                Console.WriteLine(response.IsSuccessStatusCode 
                    ? "Message sent successfully to Slack!" 
                    : $"Failed to send message to Slack. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending message to Slack: {ex.Message}");
            }
        }

        public async Task SendFailedMessageToSlack(String failedMessage)
        {
            const string slackWebhookUrl = "";

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(failedMessage), Encoding.UTF8, "application/json");
                var response = await Client.PostAsync(slackWebhookUrl, content);

                Console.WriteLine(response.IsSuccessStatusCode
                    ? "Failure message sent to Slack!"
                    : $"Failed to send failure message to Slack. Status code: {response.StatusCode}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while sending failure message to Slack: {e.Message}");
            }
        }
    }
}
