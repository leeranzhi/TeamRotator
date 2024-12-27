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
            var rotationList = new List<TaskAssignmentDto>
            {
                new TaskAssignmentDto 
                { 
                    Id = 1, 
                    TaskId = 1, 
                    TaskName = "Task1", 
                    MemberId = 1,
                    Host = "Host1",
                    SlackId = "SlackId1"
                },
                new TaskAssignmentDto 
                { 
                    Id = 2, 
                    TaskId = 2, 
                    TaskName = "Task2", 
                    MemberId = 2,
                    Host = "Host2",
                    SlackId = "SlackId2"
                }
            };
            _mockRotationService.Setup(service => service.GetRotationList()).Returns(rotationList);

            // Act
            var result = _controller.GetRotationList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedList = Assert.IsType<List<TaskAssignmentDto>>(okResult.Value); // Ensure the type matches
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
