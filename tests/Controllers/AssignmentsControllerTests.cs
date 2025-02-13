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
                new TaskAssignmentDto { /* Initialize properties */ },
                new TaskAssignmentDto { /* Initialize properties */ }
            };

            _mockRotationService.Setup(service => service.GetRotationList())
                .Returns(rotationList);

            // Act
            var result = _controller.GetRotationList();

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<TaskAssignmentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<TaskAssignmentDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
        [Fact]
        public void UpdateRotationList_ShouldReturnUpdatedTaskAssignment()
        {
            // Arrange
            var modifyAssignmentDto = new ModifyAssignmentDto();
            var updatedAssignment = new TaskAssignment();

            _mockAssignmentUpdateService.Setup(service => service.ModifyTaskAssignment(It.IsAny<int>(), It.IsAny<ModifyAssignmentDto>()))
                .Returns(updatedAssignment);

            // Act
            var result = _controller.UpdateRotationList(1, modifyAssignmentDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TaskAssignment>>(result); // 先检查 ActionResult
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // 再获取 Result 并检查 OkObjectResult
            var returnValue = Assert.IsType<TaskAssignment>(okResult.Value);
            Assert.Equal(updatedAssignment, returnValue);
        }

    }
}
