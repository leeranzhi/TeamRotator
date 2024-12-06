using Moq;
using Quartz;
using Buzz.Model;
using Buzz.Services;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.Services;

public class QuartzServiceTests
{
    [Fact]
    public async Task ConfigureJobsAsync_ShouldScheduleJobs_WhenWorkingDay()
    {
        // Arrange
        var mockSchedulerFactory = new Mock<ISchedulerFactory>();
        var mockWorkingDayCheckService = new Mock<IWorkingDayCheckService>();
        var mockScheduler = new Mock<IScheduler>();

        mockSchedulerFactory.Setup(s => s.GetScheduler(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockScheduler.Object);

        mockWorkingDayCheckService.Setup(w => w.IsWorkingDay(It.IsAny<DateTime>())).ReturnsAsync(true);

        var quartzService = new QuartzService(mockSchedulerFactory.Object, mockWorkingDayCheckService.Object);

        // Act
        await quartzService.ConfigureJobsAsync();

        // Assert
        mockSchedulerFactory.Verify(f => f.GetScheduler(It.IsAny<CancellationToken>()), Times.Once);
        mockWorkingDayCheckService.Verify(w => w.IsWorkingDay(It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task ConfigureJobsAsync_ShouldNotScheduleJobs_WhenNotWorkingDay()
    {
        // Arrange
        var mockSchedulerFactory = new Mock<ISchedulerFactory>();
        var mockWorkingDayCheckService = new Mock<IWorkingDayCheckService>();

        mockWorkingDayCheckService.Setup(w => w.IsWorkingDay(It.IsAny<DateTime>())).ReturnsAsync(false);

        var quartzService = new QuartzService(mockSchedulerFactory.Object, mockWorkingDayCheckService.Object);

        // Act
        await quartzService.ConfigureJobsAsync();

        // Assert
        mockWorkingDayCheckService.Verify(w => w.IsWorkingDay(It.IsAny<DateTime>()), Times.Once);

        mockSchedulerFactory.Verify(f => f.GetScheduler(It.IsAny<CancellationToken>()), Times.Never);
    }
}
