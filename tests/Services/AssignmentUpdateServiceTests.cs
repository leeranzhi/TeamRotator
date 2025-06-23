using Buzz;
using Buzz.Model;
using Buzz.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Task = Buzz.Model.Task;

namespace Tests.Services;

public class AssignmentUpdateServiceTests
{
    private class DbContextFactory : IDbContextFactory<RotationDbContext>
    {
        private readonly DbContextOptions<RotationDbContext> _options;

        public DbContextFactory(DbContextOptions<RotationDbContext> options)
        {
            _options = options;
        }

        public RotationDbContext CreateDbContext() => new RotationDbContext(_options);
    }
    
    [Fact]
    public void UpdateTaskAssignment_UpdatesAssignmentDatesForWeekly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RotationDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateAssignmentWeeklyTestDatabase")
            .Options;

        using (var context = new RotationDbContext(options))
        {
            context.Tasks.Add(new Task { Id = 1,  RotationRule= "daily" });
            context.TaskAssignments.Add(new TaskAssignment
            {
                Id = 1,
                TaskId = 2,
                StartDate = new DateOnly(2024, 9, 1),
                EndDate = new DateOnly(2024, 9, 1),
                MemberId = 1,
            });
            context.Members.AddRange(
                new Member { Id = 1, SlackId = "P00000NN999", Host = "zhiqiao" },
                new Member { Id = 2, SlackId = "U66666NN5DX", Host = "yahui" }
            );
            context.SaveChanges();
        }

        var fakeCurrentDate = new DateOnly(2024, 9, 2);
        var timeProvider = new TestTimeProvider(fakeCurrentDate);

        // Act
        using (var context = new RotationDbContext(options))
        {
            var mockLogger = new Mock<ILogger<AssignmentUpdateService>>();
            var service = new AssignmentUpdateService(new DbContextFactory(options), null, mockLogger.Object,null, timeProvider);

            var assignment = new TaskAssignment
            {
                Id = 1, TaskId = 2, MemberId = 1, StartDate = new DateOnly(2024, 9, 1),
                EndDate = new DateOnly(2024, 9, 1)
            };
            service.UpdateTaskAssignment(assignment);

            // Assert
            var updatedAssignment = context.TaskAssignments.Single(a => a.Id == 1);
            Assert.Equal(new DateOnly(2024, 9, 2), updatedAssignment.StartDate);
            Assert.Equal(new DateOnly(2024, 9, 2), updatedAssignment.EndDate);
        }
    }
}