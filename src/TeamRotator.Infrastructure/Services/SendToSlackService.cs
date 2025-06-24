using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace TeamRotator.Infrastructure.Services;

public class SendToSlackService
{
    private readonly HttpClient _httpClient;
    private readonly string _slackWebhookUrl;
    private readonly ILogger<SendToSlackService> _logger;

    public SendToSlackService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SendToSlackService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _slackWebhookUrl = configuration["Slack:WebhookUrl"] ?? throw new InvalidOperationException("Slack webhook URL not configured");
    }

    public async Task SendSlackMessage()
    {
        try
        {
            _logger.LogInformation("Sending Slack message");
            
            var message = new { text = "Today's task assignments have been updated." };
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_slackWebhookUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send message to Slack. Status code: {response.StatusCode}");
            }

            _logger.LogInformation("Slack message sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            throw;
        }
    }
} 