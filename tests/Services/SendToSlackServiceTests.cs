using System.Net;
using System.Text.Json;
using Buzz;
using Buzz.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

public class SendToSlackServiceTests
{
    [Fact]
    public async Task SendFailedMessageToSlack_ShouldSendFailureMessageToPersonalSlackUrl()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockContextFactory = new Mock<IDbContextFactory<RotationDbContext>>();

        // Setup configuration
        mockConfiguration.Setup(c => c["Slack:PersonalWebhookUrl"]).Returns("https://hooks.slack.com/services/TEST/PERSONAL/WEBHOOK");

        // Mock HttpClient
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://hooks.slack.com/services/TEST/PERSONAL/WEBHOOK") &&
                    VerifyRequestBody(req.Content, "Test failure message")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handler.Object);
        mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Create service instance
        var sendToSlackService = new SendToSlackService(
            mockContextFactory.Object,
            mockHttpClientFactory.Object,
            mockConfiguration.Object
        );

        var failureMessage = "Test failure message";

        // Act
        await sendToSlackService.SendFailedMessageToSlack(failureMessage);

        // Assert
        handler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("https://hooks.slack.com/services/TEST/PERSONAL/WEBHOOK")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    private bool VerifyRequestBody(HttpContent content, string expectedMessage)
    {
        var jsonString = content.ReadAsStringAsync().Result;
        var deserialized = JsonSerializer.Deserialize<JsonElement>(jsonString);
        return deserialized.GetProperty("text").GetString() == expectedMessage;
    }
}
