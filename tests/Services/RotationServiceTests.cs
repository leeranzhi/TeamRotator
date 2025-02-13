using Buzz;
using Buzz.Dto;
using Buzz.Model;
using Microsoft.EntityFrameworkCore;
using Moq;
using Buzz.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Task = Buzz.Model.Task;

namespace Tests.Services;

public class RotationServiceTests
{
    [Fact]
    public void GetRotationList_ReturnsCorrectlyOrderedTaskList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RotationDbContext>()
            .UseInMemoryDatabase(databaseName: "RotationTestDb")
            .Options;

        var mockContext = new RotationDbContext(options);

        // Seed data
        mockContext.Members.AddRange(
            new Member { Id = 1, Host = "zhen", SlackId = "1111" },
            new Member { Id = 2, Host = "zhiqiao", SlackId = "2222" },
            new Member { Id = 3, Host = "yahui", SlackId = "3333" },
            new Member { Id = 4, Host = "guoqing", SlackId = "4444" },
            new Member { Id = 5, Host = "jinglan", SlackId = "5555" }
        );

        mockContext.Tasks.AddRange(
            new Task { Id = 1, TaskName = "Retro" },
            new Task { Id = 2, TaskName = "English word" },
            new Task { Id = 3, TaskName = "English word(Day + 1)" },
            new Task { Id = 4, TaskName = "English word(Day + 2)" },
            new Task { Id = 5, TaskName = "Standup" },
            new Task { Id = 6, TaskName = "Tech huddle" }
        );

        mockContext.TaskAssignments.AddRange(
            new TaskAssignment { Id = 7, MemberId = 1, TaskId = 1 },
            new TaskAssignment { Id = 8, MemberId = 2, TaskId = 2 },
            new TaskAssignment { Id = 9, MemberId = 3, TaskId = 3 },
            new TaskAssignment { Id = 10, MemberId = 4, TaskId = 4 },
            new TaskAssignment { Id = 11, MemberId = 5, TaskId = 5 },
            new TaskAssignment { Id = 12, MemberId = 5, TaskId = 6 }
        );

        mockContext.SaveChanges();

        var mockFactory = new Mock<IDbContextFactory<RotationDbContext>>();
        mockFactory.Setup(f => f.CreateDbContext()).Returns(mockContext);

        var mockAssignmentUpdateService = new Mock<IAssignmentUpdateService>();
        var mockLogger = new Mock<ILogger<AssignmentUpdateService>>();

        var service = new RotationService(mockFactory.Object, mockAssignmentUpdateService.Object, mockLogger.Object);

        // Act
        var result = service.GetRotationList();

        // Assert
        var expected = new List<TaskAssignmentDto>
        {
            new TaskAssignmentDto { Id = 7, TaskId = 1, TaskName = "Retro", MemberId = 1, Host = "zhen", SlackId = "1111" },
            new TaskAssignmentDto { Id = 8, TaskId = 2, TaskName = "English word", MemberId = 2, Host = "zhiqiao", SlackId = "2222" },
            new TaskAssignmentDto { Id = 9, TaskId = 3, TaskName = "English word(Day + 1)", MemberId = 3, Host = "yahui", SlackId = "3333" },
            new TaskAssignmentDto { Id = 10, TaskId = 4, TaskName = "English word(Day + 2)", MemberId = 4, Host = "guoqing", SlackId = "4444" },
            new TaskAssignmentDto { Id = 11, TaskId = 5, TaskName = "Standup", MemberId = 5, Host = "jinglan", SlackId = "5555" },
            new TaskAssignmentDto { Id = 12, TaskId = 6, TaskName = "Tech huddle", MemberId = 5, Host = "jinglan", SlackId = "5555" }
        };

        // Compare each element in the list
        Assert.Equal(expected.Count, result.Count);

        for (int i = 0; i < expected.Count; i++)
        {
            var expectedItem = expected[i];
            var resultItem = result[i];

            Assert.Equal(expectedItem.Id, resultItem.Id);
            Assert.Equal(expectedItem.TaskId, resultItem.TaskId);
            Assert.Equal(expectedItem.TaskName, resultItem.TaskName);
            Assert.Equal(expectedItem.MemberId, resultItem.MemberId);
            Assert.Equal(expectedItem.Host, resultItem.Host);
            Assert.Equal(expectedItem.SlackId, resultItem.SlackId);
        }
    }
}
