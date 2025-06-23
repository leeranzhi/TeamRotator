using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using TeamRotator.Infrastructure.Services;

namespace TeamRotator.Tests.Services;

public class WorkingDayCheckServiceTests : TestBase
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly WorkingDayCheckService _service;

    public WorkingDayCheckServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _service = new WorkingDayCheckService(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            CreateLogger<WorkingDayCheckService>().Object);
    }

    [Fact]
    public async Task IsWorkingDayCheck_ReturnsTrue_WhenNotHoliday()
    {
        // Arrange
        var date = new DateTime(2024, 3, 20);
        var apiUrl = "http://example.com/api";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"2024-01-01\": 1}")
        };

        _configurationMock.Setup(c => c["HolidayApiSettings:Url"]).Returns(apiUrl);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(client);

        // Act
        var result = await _service.IsWorkingDayCheck(date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsWorkingDayCheck_ReturnsFalse_WhenHoliday()
    {
        // Arrange
        var date = new DateTime(2024, 3, 20);
        var apiUrl = "http://example.com/api";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"2024-03-20\": 1}")
        };

        _configurationMock.Setup(c => c["HolidayApiSettings:Url"]).Returns(apiUrl);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(client);

        // Act
        var result = await _service.IsWorkingDayCheck(date);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsWorkingDayCheck_ReturnsTrue_WhenApiCallFails()
    {
        // Arrange
        var date = new DateTime(2024, 3, 20);
        var apiUrl = "http://example.com/api";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _configurationMock.Setup(c => c["HolidayApiSettings:Url"]).Returns(apiUrl);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(client);

        // Act
        var result = await _service.IsWorkingDayCheck(date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsWorkingDayCheck_ReturnsTrue_WhenDeserializationFails()
    {
        // Arrange
        var date = new DateTime(2024, 3, 20);
        var apiUrl = "http://example.com/api";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("invalid json")
        };

        _configurationMock.Setup(c => c["HolidayApiSettings:Url"]).Returns(apiUrl);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(client);

        // Act
        var result = await _service.IsWorkingDayCheck(date);

        // Assert
        Assert.True(result);
    }
} 