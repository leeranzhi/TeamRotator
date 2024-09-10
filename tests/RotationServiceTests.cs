using Buzz;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class RotationServiceTests
{
    [Fact]
    public void GetRotationList_ReturnsCorrectlyOrderedTaskList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RotationDbContext>()
            .UseInMemoryDatabase(databaseName: "RotationTestDb")
            .Options;

        using (var context = new RotationDbContext(options))
        {
            // Seed data
            context.TaskAssignments.AddRange(
                new TaskAssignment { Id = 1, TaskName = "Retro", PersonName = "jinglan" },
                new TaskAssignment { Id = 2, TaskName = "English word", PersonName = "zhen" },
                new TaskAssignment { Id = 3, TaskName = "English word(Day + 1)", PersonName = "yonglong" },
                new TaskAssignment { Id = 4, TaskName = "English word(Day + 2)", PersonName = "jinglan" },
                new TaskAssignment { Id = 5, TaskName = "Standup", PersonName = "zhen" },
                new TaskAssignment { Id = 6, TaskName = "Tech huddle", PersonName = "zhen" }
            );
            context.SaveChanges();
        }

        using (var context = new RotationDbContext(options))
        {
            var service = new RotationService(context);

            // Act
            var result = service.GetRotationList();

            // Assert
            var expected = new List<string>
            {
                "Retro: jinglan",
                "English word: zhen",
                "English word(Day + 1): yonglong",
                "English word(Day + 2): jinglan",
                "Standup: zhen",
                "Tech huddle: zhen"
            };

            Assert.Equal(expected, result);
        }
    }
}