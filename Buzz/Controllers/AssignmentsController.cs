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
        private readonly RotationService _rotationService;
        private readonly AssignmentUpdateService _assignmentUpdateService;

        public AssignmentsController(RotationService rotationService,AssignmentUpdateService assignmentUpdateService)
        {
            _rotationService = rotationService;
            _assignmentUpdateService = assignmentUpdateService;
        }

        [HttpGet]
        public ActionResult<List<TaskAssignment>> GetRotationList()
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