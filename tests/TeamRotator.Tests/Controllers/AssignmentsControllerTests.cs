using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamRotator.Api.Controllers;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;
using Xunit;

namespace TeamRotator.Tests.Controllers;

public class AssignmentsControllerTests
{
    private readonly Mock<IRotationService> _rotationServiceMock;
    private readonly Mock<IAssignmentUpdateService> _assignmentUpdateServiceMock;
    private readonly AssignmentsController _controller;

    public AssignmentsControllerTests()
    {
        _rotationServiceMock = new Mock<IRotationService>();
        _assignmentUpdateServiceMock = new Mock<IAssignmentUpdateService>();
        _controller = new AssignmentsController(
            _rotationServiceMock.Object,
            _assignmentUpdateServiceMock.Object);
    }

    [Fact]
    public void GetRotationList_ReturnsOkResult()
    {
        // Arrange
        var expectedList = new List<TaskAssignmentDto>
        {
            new() { Id = 1, TaskId = 1, TaskName = "Task1", MemberId = 1, Host = "user1", SlackId = "U1" }
        };
        _rotationServiceMock.Setup(s => s.GetRotationList()).Returns(expectedList);

        // Act
        var result = _controller.GetRotationList();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<TaskAssignmentDto>>(okResult.Value);
        Assert.Equal(expectedList, returnValue);
    }

    [Fact]
    public void GetRotationList_HandlesException()
    {
        // Arrange
        _rotationServiceMock.Setup(s => s.GetRotationList())
            .Throws(new InvalidOperationException("Test error"));

        // Act
        var result = _controller.GetRotationList();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Test error", badRequestResult.Value);
    }

    [Fact]
    public void UpdateRotationList_ReturnsOkResult()
    {
        // Arrange
        var dto = new ModifyAssignmentDto { Host = "user1" };
        var assignment = new TaskAssignment { Id = 1, TaskId = 1, MemberId = 1 };
        _assignmentUpdateServiceMock.Setup(s => s.ModifyTaskAssignment(1, dto))
            .Returns(assignment);

        // Act
        var result = _controller.UpdateRotationList(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<TaskAssignment>(okResult.Value);
        Assert.Equal(assignment, returnValue);
    }

    [Fact]
    public void UpdateRotationList_HandlesException()
    {
        // Arrange
        var dto = new ModifyAssignmentDto { Host = "user1" };
        _assignmentUpdateServiceMock.Setup(s => s.ModifyTaskAssignment(1, dto))
            .Throws(new InvalidOperationException("Test error"));

        // Act
        var result = _controller.UpdateRotationList(1, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Test error", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateAssignments_ReturnsOkResult()
    {
        // Arrange
        _rotationServiceMock.Setup(s => s.UpdateTaskAssignmentList())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAssignments();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAssignments_HandlesException()
    {
        // Arrange
        _rotationServiceMock.Setup(s => s.UpdateTaskAssignmentList())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        var result = await _controller.UpdateAssignments();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Test error", badRequestResult.Value);
    }
} 