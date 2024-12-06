using Buzz.Jobs;
using Buzz.Services;
using Buzz.Model;
using Moq;
using Quartz;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.Services;

public class QuartzServiceTests
{
    [Fact]
    public async Task ConfigureJobsAsync_SchedulesJobs_WhenIsWorkingDay()
    {
        // Arrange
        var schedulerMock = new Mock<IScheduler>();
        var workingDayCheckServiceMock = new Mock<IWorkingDayCheckService>();

        workingDayCheckServiceMock
            .Setup(w => w.IsWorkingDay(It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var quartzService = new QuartzService(schedulerMock.Object, workingDayCheckServiceMock.Object);

        // Act
        await quartzService.ConfigureJobsAsync();

        // Assert
        schedulerMock.Verify(s => s.ScheduleJob(
            It.Is<IJobDetail>(j => j.Key.Name == "AssignmentUpdateJob" && j.JobType == typeof(AssignmentUpdateJob)),
            It.Is<ITrigger>(t => t.Key.Name == "AssignmentUpdateJob-trigger"),
            default), Times.Once);

        schedulerMock.Verify(s => s.ScheduleJob(
            It.Is<IJobDetail>(j => j.Key.Name == "SendToSlackJob" && j.JobType == typeof(SendToSlackJob)),
            It.Is<ITrigger>(t => t.Key.Name == "SendToSlackJob-trigger"),
            default), Times.Once);

        workingDayCheckServiceMock.Verify(w => w.IsWorkingDay(It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task ConfigureJobsAsync_DoesNotScheduleJobs_WhenIsNotWorkingDay()
    {
        // Arrange
        var schedulerMock = new Mock<IScheduler>();
        var workingDayCheckServiceMock = new Mock<IWorkingDayCheckService>();

        workingDayCheckServiceMock
            .Setup(w => w.IsWorkingDay(It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var quartzService = new QuartzService(schedulerMock.Object, workingDayCheckServiceMock.Object);

        // Act
        await quartzService.ConfigureJobsAsync();

        // Assert
        schedulerMock.Verify(s => s.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), default), Times.Never);

        workingDayCheckServiceMock.Verify(w => w.IsWorkingDay(It.IsAny<DateTime>()), Times.Once);
    }
}
