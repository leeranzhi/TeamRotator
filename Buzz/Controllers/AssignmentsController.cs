using Buzz.Dto;
using Buzz.Model;
using Buzz.Services;
using Microsoft.AspNetCore.Mvc;

namespace Buzz.Controllers
{
    [ApiController]
    [Route("assignments")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IRotationService _rotationService;
        private readonly IAssignmentUpdateService _assignmentUpdateService;

        public AssignmentsController(IRotationService rotationService, IAssignmentUpdateService assignmentUpdateService)
        {
            _rotationService = rotationService;
            _assignmentUpdateService = assignmentUpdateService;
        }

        [HttpGet]
        public ActionResult<List<TaskAssignmentDto>> GetRotationList()
        {
            var rotationList = _rotationService.GetRotationList();
            return Ok(rotationList); 
        }

        [HttpPut("{id}")]
        public ActionResult<TaskAssignment> UpdateRotationList(int id, [FromBody] ModifyAssignmentDto modifyAssignmentDto)
        {
            var updatedAssignment = _assignmentUpdateService.ModifyTaskAssignment(id, modifyAssignmentDto);
            
            return Ok(updatedAssignment);
        }
    }
}