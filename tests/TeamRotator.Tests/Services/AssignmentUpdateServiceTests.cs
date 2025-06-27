using Moq;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using TeamRotator.Infrastructure.Services;
using Xunit;
using TaskEntity = TeamRotator.Core.Entities.Task;
using Task = System.Threading.Tasks.Task;

namespace TeamRotator.Tests.Services;

public class AssignmentUpdateServiceTests : TestBase
{
    private readonly Mock<SendToSlackService> _slackServiceMock;
    private readonly Mock<IWorkingDayCheckService> _workingDayCheckServiceMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly AssignmentUpdateService _service;
    private readonly RotationDbContext _context;

    public AssignmentUpdateServiceTests()
    {
        _slackServiceMock = new Mock<SendToSlackService>();
        _workingDayCheckServiceMock = new Mock<IWorkingDayCheckService>();
        _timeProviderMock = new Mock<ITimeProvider>();
        var contextFactory = CreateDbContextFactory();
        _context = contextFactory.CreateDbContext();
        _service = new AssignmentUpdateService(
            contextFactory,
            _slackServiceMock.Object,
            CreateLogger<AssignmentUpdateService>().Object,
            _workingDayCheckServiceMock.Object,
            _timeProviderMock.Object);
    }

    [Fact]
    public async Task UpdateTaskAssignment_UpdatesAssignmentForDailyTask()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        _timeProviderMock.Setup(p => p.GetCurrentDate()).Returns(today);
        _workingDayCheckServiceMock.Setup(s => s.IsWorkingDayCheck(It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var members = new List<Member>
        {
            new() { Id = 1, Host = "user1", SlackId = "U1" },
            new() { Id = 2, Host = "user2", SlackId = "U2" }
        };
        var task = new TaskEntity { Id = 1, TaskName = "Task1", RotationRule = "daily" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(-1)
        };

        _context.Members.AddRange(members);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act
        await _service.UpdateTaskAssignment(assignment);

        // Assert
        var updatedAssignment = _context.TaskAssignments.First();
        Assert.Equal(2, updatedAssignment.MemberId);
        Assert.Equal(today, updatedAssignment.StartDate);
        Assert.Equal(today, updatedAssignment.EndDate);
    }

    [Fact]
    public async Task UpdateTaskAssignment_UpdatesAssignmentForWeeklyTask()
    {
        // Arrange
        var today = DateOnly.FromDateTime(new DateTime(2024, 1, 1)); // Monday
        _timeProviderMock.Setup(p => p.GetCurrentDate()).Returns(today);

        var members = new List<Member>
        {
            new() { Id = 1, Host = "user1", SlackId = "U1" },
            new() { Id = 2, Host = "user2", SlackId = "U2" }
        };
        var task = new TaskEntity { Id = 1, TaskName = "Task1", RotationRule = "weekly_monday" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = today.AddDays(-7),
            EndDate = today.AddDays(-1)
        };

        _context.Members.AddRange(members);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act
        await _service.UpdateTaskAssignment(assignment);

        // Assert
        var updatedAssignment = _context.TaskAssignments.First();
        Assert.Equal(2, updatedAssignment.MemberId);
        Assert.Equal(today.AddDays(1), updatedAssignment.StartDate);
        Assert.Equal(today.AddDays(7), updatedAssignment.EndDate);
    }

    [Fact]
    public async Task UpdateTaskAssignment_UpdatesAssignmentForBiweeklyTask()
    {
        // Arrange
        var today = DateOnly.FromDateTime(new DateTime(2024, 1, 1)); // Monday
        _timeProviderMock.Setup(p => p.GetCurrentDate()).Returns(today);

        var members = new List<Member>
        {
            new() { Id = 1, Host = "user1", SlackId = "U1" },
            new() { Id = 2, Host = "user2", SlackId = "U2" }
        };
        var task = new TaskEntity { Id = 1, TaskName = "Task1", RotationRule = "biweekly_monday" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = today.AddDays(-14),
            EndDate = today.AddDays(-1)
        };

        _context.Members.AddRange(members);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act
        await _service.UpdateTaskAssignment(assignment);

        // Assert
        var updatedAssignment = _context.TaskAssignments.First();
        Assert.Equal(2, updatedAssignment.MemberId);
        Assert.Equal(today.AddDays(8), updatedAssignment.StartDate);
        Assert.Equal(today.AddDays(21), updatedAssignment.EndDate);
    }

    [Fact]
    public async Task UpdateTaskAssignment_SkipsNonWorkingDay()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        _timeProviderMock.Setup(p => p.GetCurrentDate()).Returns(today);
        _workingDayCheckServiceMock.Setup(s => s.IsWorkingDayCheck(It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var members = new List<Member>
        {
            new() { Id = 1, Host = "user1", SlackId = "U1" },
            new() { Id = 2, Host = "user2", SlackId = "U2" }
        };
        var task = new TaskEntity { Id = 1, TaskName = "Task1", RotationRule = "daily" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(-1)
        };

        _context.Members.AddRange(members);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act
        await _service.UpdateTaskAssignment(assignment);

        // Assert
        var updatedAssignment = _context.TaskAssignments.First();
        Assert.Equal(1, updatedAssignment.MemberId);
    }

    [Fact]
    public void ModifyTaskAssignment_UpdatesAssignment()
    {
        // Arrange
        var member = new Member { Id = 1, Host = "user1", SlackId = "U1" };
        var task = new TaskEntity { Id = 1, TaskName = "Task1" };
        var assignment = new TaskAssignment { Id = 1, TaskId = 1, MemberId = 2 };

        _context.Members.Add(member);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        var dto = new ModifyAssignmentDto { Host = "user1" };

        // Act
        var result = _service.ModifyTaskAssignment(1, dto);

        // Assert
        Assert.Equal(1, result.MemberId);
    }

    [Fact]
    public void ModifyTaskAssignment_ThrowsWhenAssignmentNotFound()
    {
        // Arrange
        var dto = new ModifyAssignmentDto { Host = "user1" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.ModifyTaskAssignment(1, dto));
    }

    [Fact]
    public void ModifyTaskAssignment_ThrowsWhenMemberNotFound()
    {
        // Arrange
        var assignment = new TaskAssignment { Id = 1, TaskId = 1, MemberId = 1 };
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        var dto = new ModifyAssignmentDto { Host = "nonexistent" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.ModifyTaskAssignment(1, dto));
    }

    [Fact]
    public async Task UpdateTaskAssignment_SendsFailureMessageOnError()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        _timeProviderMock.Setup(p => p.GetCurrentDate()).Returns(today);
        _workingDayCheckServiceMock.Setup(s => s.IsWorkingDayCheck(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Test error"));

        var task = new TaskEntity { Id = 1, TaskName = "Task1", RotationRule = "daily" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(-1)
        };

        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.UpdateTaskAssignment(assignment));
        _slackServiceMock.Verify(s => s.SendFailedMessageToSlack(It.IsAny<string>()), Times.Once);
    }
} 