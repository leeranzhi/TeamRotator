using System.Net;
using Buzz.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Tests.Services
{
    public class WorkingDayCheckServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly WorkingDayCheckService _service;

        public WorkingDayCheckServiceTests()
        {
            Mock<IConfiguration> mockConfiguration = new();
            mockConfiguration.Setup(x => x["HolidayApiSettings:Url"]).Returns("http://test.com");
            
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(_mockHttpMessageHandler.Object);
            
            Mock<IHttpClientFactory> mockHttpClientFactory = new();
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(client);
            var mockLogger = new Mock<ILogger<AssignmentUpdateService>>();

            _service = new WorkingDayCheckService(mockConfiguration.Object, mockHttpClientFactory.Object, mockLogger.Object);
        }

        [Fact]
        public async Task IsWorkingDay_Weekend_ReturnsFalse()
        {
            // Arrange
            var saturday = new DateTime(2024, 3, 23);
            SetupMockResponse(saturday.Year, "[]");

            // Act
            var result = await _service.IsWorkingDayCheck(saturday);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsWorkingDay_Weekday_ReturnsTrue()
        {
            // Arrange
            var monday = new DateTime(2024, 3, 25);
            SetupMockResponse(monday.Year, "[]");
            
            // Act
            var result = await _service.IsWorkingDayCheck(monday);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsWorkingDay_Holiday_ReturnsFalse()
        {
            // Arrange
            var holidayDate = new DateTime(2024, 1, 1);
            var holidayJson = @"{
                ""days"": [
                    {
                        ""name"": ""New Year's Day"",
                        ""date"": ""2024-01-01"",
                        ""isOffDay"": true
                    }
                ]
            }";
            SetupMockResponse(holidayDate.Year, holidayJson);

            // Act
            var result = await _service.IsWorkingDayCheck(holidayDate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsWorkingDay_WorkingHoliday_ReturnsTrue()
        {
            // Arrange
            var workingHolidayDate = new DateTime(2024, 2, 18);
            var holidayJson = @"{
                ""days"": [
                    {
                        ""name"": ""Spring Festival Makeup Day"",
                        ""date"": ""2024-02-18"",
                        ""isOffDay"": false
                    }
                ]
            }";
            SetupMockResponse(workingHolidayDate.Year, holidayJson);

            // Act
            var result = await _service.IsWorkingDayCheck(workingHolidayDate);

            // Assert
            Assert.True(result);
        }

        private void SetupMockResponse(int year, string content)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"{year}.json")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }
    }
}