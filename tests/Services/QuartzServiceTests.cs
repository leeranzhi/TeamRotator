using Buzz.Model;
using Moq;
using Quartz;
using Buzz.Services;
using Microsoft.Extensions.Logging;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.Services;

public class QuartzServiceTests
{
    private readonly Mock<ISchedulerFactory> _mockSchedulerFactory;
    private readonly Mock<IWorkingDayCheckService> _mockWorkingDayCheckService;
    private readonly Mock<IScheduler> _mockScheduler;
    private readonly Mock<ILogger<QuartzService>> _mockLogger;
    private readonly QuartzService _quartzService;

    public QuartzServiceTests()
    {
        _mockSchedulerFactory = new Mock<ISchedulerFactory>();
        _mockWorkingDayCheckService = new Mock<IWorkingDayCheckService>();
        _mockScheduler = new Mock<IScheduler>();
        _mockLogger = new Mock<ILogger<QuartzService>>();

        _mockSchedulerFactory.Setup(s => s.GetScheduler(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockScheduler.Object);
        
        _mockScheduler.Setup(s => s.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        _quartzService = new QuartzService(
            _mockSchedulerFactory.Object, 
            _mockWorkingDayCheckService.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task ConfigureJobsAsync_ShouldScheduleJobs_WhenWorkingDay()
    {
        // Arrange
        _mockWorkingDayCheckService.Setup(w => w.IsWorkingDayCheck(It.IsAny<DateTime>())).ReturnsAsync(true);

        // Act
        await _quartzService.ConfigureJobsAsync();

        // Assert
        _mockWorkingDayCheckService.Verify(w => w.IsWorkingDayCheck(It.IsAny<DateTime>()), Times.Once);
        _mockSchedulerFactory.Verify(f => f.GetScheduler(It.IsAny<CancellationToken>()), Times.Once);
        _mockScheduler.Verify(s => s.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ConfigureJobsAsync_ShouldNotScheduleJobs_WhenNotWorkingDay()
    {
        // Arrange
        _mockWorkingDayCheckService.Setup(w => w.IsWorkingDayCheck(It.IsAny<DateTime>())).ReturnsAsync(false);

        // Act
        await _quartzService.ConfigureJobsAsync();

        // Assert
        _mockWorkingDayCheckService.Verify(w => w.IsWorkingDayCheck(It.IsAny<DateTime>()), Times.Once);
        _mockSchedulerFactory.Verify(f => f.GetScheduler(It.IsAny<CancellationToken>()), Times.Never);
        _mockScheduler.Verify(s => s.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
