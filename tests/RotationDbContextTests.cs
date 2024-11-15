using Buzz;
using Buzz.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests
{
    public class RotationDbContextTests
    {
        [Fact]
        public void CanInsertTaskAssignmentIntoDatabase()
        {
            var options = new DbContextOptionsBuilder<RotationDbContext>()
                .UseInMemoryDatabase(databaseName: "Postgres")
                .Options;

            using (var context = new RotationDbContext(options))
            {
                var taskAssignment = new TaskAssignment
                {
                    Id = 5,
                    TaskId = 5,
                    StartDate = new DateOnly(2024, 9, 9),
                    EndDate = new DateOnly(2024, 9, 16),
                    MemberId = 4
                };

                context.TaskAssignments.Add(taskAssignment);
                context.SaveChanges();
            }

            using (var context = new RotationDbContext(options))
            {
                Assert.Equal(1, context.TaskAssignments.Count());
            }
        }
    }
}