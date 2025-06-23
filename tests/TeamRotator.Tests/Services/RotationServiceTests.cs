using Moq;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using TeamRotator.Infrastructure.Services;

namespace TeamRotator.Tests.Services;

public class RotationServiceTests : TestBase
{
    private readonly Mock<IAssignmentUpdateService> _assignmentUpdateServiceMock;
    private readonly RotationService _service;
    private readonly RotationDbContext _context;

    public RotationServiceTests()
    {
        _assignmentUpdateServiceMock = new Mock<IAssignmentUpdateService>();
        var contextFactory = CreateDbContextFactory();
        _context = contextFactory.CreateDbContext();
        _service = new RotationService(
            contextFactory,
            _assignmentUpdateServiceMock.Object,
            CreateLogger<RotationService>().Object);
    }

    [Fact]
    public void GetRotationList_ReturnsCorrectList()
    {
        // Arrange
        var member = new Member { Id = 1, Host = "user1", SlackId = "U1" };
        var task = new Task { Id = 1, TaskName = "Task1" };
        var assignment = new TaskAssignment { Id = 1, TaskId = 1, MemberId = 1 };

        _context.Members.Add(member);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act
        var result = _service.GetRotationList();

        // Assert
        Assert.Single(result);
        var firstAssignment = result[0];
        Assert.Equal(1, firstAssignment.Id);
        Assert.Equal(1, firstAssignment.TaskId);
        Assert.Equal("Task1", firstAssignment.TaskName);
        Assert.Equal(1, firstAssignment.MemberId);
        Assert.Equal("user1", firstAssignment.Host);
        Assert.Equal("U1", firstAssignment.SlackId);
    }

    [Fact]
    public async Task UpdateTaskAssignmentList_UpdatesAssignments()
    {
        // Arrange
        var member = new Member { Id = 1, Host = "user1", SlackId = "U1" };
        var task = new Task { Id = 1, TaskName = "Task1" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };

        _context.Members.Add(member);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        _assignmentUpdateServiceMock.Setup(s => s.UpdateTaskAssignment(It.IsAny<TaskAssignment>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateTaskAssignmentList();

        // Assert
        _assignmentUpdateServiceMock.Verify(
            s => s.UpdateTaskAssignment(It.Is<TaskAssignment>(a => a.Id == 1)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAssignmentList_SkipsCurrentAssignments()
    {
        // Arrange
        var member = new Member { Id = 1, Host = "user1", SlackId = "U1" };
        var task = new Task { Id = 1, TaskName = "Task1" };
        var assignment = new TaskAssignment
        {
            Id = 1,
            TaskId = 1,
            MemberId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        _context.Members.Add(member);
        _context.Tasks.Add(task);
        _context.TaskAssignments.Add(assignment);
        _context.SaveChanges();

        // Act
        await _service.UpdateTaskAssignmentList();

        // Assert
        _assignmentUpdateServiceMock.Verify(
            s => s.UpdateTaskAssignment(It.IsAny<TaskAssignment>()),
            Times.Never);
    }
} 