using Microsoft.AspNetCore.Mvc;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;

namespace TeamRotator.Api.Controllers;

public class AssignmentsController : BaseController
{
    private readonly IRotationService _rotationService;
    private readonly IAssignmentUpdateService _assignmentUpdateService;

    public AssignmentsController(
        ILogger<AssignmentsController> logger,
        IRotationService rotationService,
        IAssignmentUpdateService assignmentUpdateService)
        : base(logger)
    {
        _rotationService = rotationService;
        _assignmentUpdateService = assignmentUpdateService;
    }

    [HttpGet]
    public ActionResult<List<TaskAssignmentDto>> GetRotationList()
    {
        try
        {
            var rotationList = _rotationService.GetRotationList();
            return Ok(rotationList);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<TaskAssignmentDto> ModifyAssignment(int id, [FromBody] ModifyAssignmentDto modifyAssignmentDto)
    {
        try
        {
            var assignment = _assignmentUpdateService.ModifyTaskAssignment(id, modifyAssignmentDto);
            return Ok(assignment);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateAssignments()
    {
        try
        {
            await _rotationService.UpdateTaskAssignmentList();
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
} 