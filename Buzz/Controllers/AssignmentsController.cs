using Buzz.Dto;
using Buzz.Model;
using Microsoft.AspNetCore.Mvc;

namespace Buzz.Controllers;

[ApiController]
[Route("assignments")]
public class AssignmentsController(IRotationService rotationService, IAssignmentUpdateService assignmentUpdateService)
    : ControllerBase
{
    [HttpGet]
    public ActionResult<List<TaskAssignmentDto>> GetRotationList()
    {
        var rotationList = rotationService.GetRotationList();
        return Ok(rotationList); 
    }

    [HttpPut("{id}")]
    public ActionResult<TaskAssignment> UpdateRotationList(int id, [FromBody] ModifyAssignmentDto modifyAssignmentDto)
    {
        var updatedAssignment = assignmentUpdateService.ModifyTaskAssignment(id, modifyAssignmentDto);
        
        return Ok(updatedAssignment);
    }
}
    