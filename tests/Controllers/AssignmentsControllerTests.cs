using Buzz.Dto;
using Buzz.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Buzz.Controllers;

namespace Tests.Controllers
{
    public class AssignmentsControllerTests
    {
        private readonly Mock<IRotationService> _mockRotationService;
        private readonly Mock<IAssignmentUpdateService> _mockAssignmentUpdateService;
        private readonly AssignmentsController _controller;

        public AssignmentsControllerTests()
        {
            _mockRotationService = new Mock<IRotationService>();
            _mockAssignmentUpdateService = new Mock<IAssignmentUpdateService>();
            _controller = new AssignmentsController(_mockRotationService.Object, _mockAssignmentUpdateService.Object);
        }

        [Fact]
        public void GetRotationList_ShouldReturnOkWithRotationList()
        {
            // Arrange
            var rotationList = new List<object>
            {
                new { Id = 1, TaskId = 1, StartDate = DateOnly.Parse("2024-01-01"), EndDate = DateOnly.Parse("2024-01-07"), MemberId = 1 },
                new { Id = 2, TaskId = 2, StartDate = DateOnly.Parse("2024-01-08"), EndDate = DateOnly.Parse("2024-01-14"), MemberId = 2 }
            };
            _mockRotationService.Setup(service => service.GetRotationList()).Returns(rotationList);

            // Act
            var result = _controller.GetRotationList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedList = Assert.IsType<List<object>>(okResult.Value);
            Assert.Equal(rotationList.Count, returnedList.Count);
        }

        [Fact]
        public void UpdateRotationList_ShouldReturnOkWithUpdatedAssignment()
        {
            // Arrange
            var id = 1;
            var modifyAssignmentDto = new ModifyAssignmentDto { Host = "NewHost" };
            var updatedAssignment = new TaskAssignment { Id = id, MemberId = 1, TaskId = 1, StartDate = DateOnly.Parse("2024-01-01"), EndDate = DateOnly.Parse("2024-01-07") };

            _mockAssignmentUpdateService
                .Setup(service => service.ModifyTaskAssignment(id, modifyAssignmentDto))
                .Returns(updatedAssignment);

            // Act
            var result = _controller.UpdateRotationList(id, modifyAssignmentDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAssignment = Assert.IsType<TaskAssignment>(okResult.Value);
            Assert.Equal(updatedAssignment, returnedAssignment);
        }
    }
}
