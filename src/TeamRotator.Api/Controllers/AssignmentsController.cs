using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Api.Controllers;

public class AssignmentsController : BaseController
{
    private readonly IRotationService _rotationService;
    private readonly IAssignmentUpdateService _assignmentUpdateService;
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;

    public AssignmentsController(
        ILogger<AssignmentsController> logger,
        IRotationService rotationService,
        IAssignmentUpdateService assignmentUpdateService,
        IDbContextFactory<RotationDbContext> contextFactory)
        : base(logger)
    {
        _rotationService = rotationService;
        _assignmentUpdateService = assignmentUpdateService;
        _contextFactory = contextFactory;
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
            return HandleException<List<TaskAssignmentDto>>(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskAssignmentDto>> ModifyAssignment(int id, [FromBody] ModifyAssignmentDto modifyAssignmentDto)
    {
        try
        {
            var assignment = _assignmentUpdateService.ModifyTaskAssignment(id, modifyAssignmentDto);
            
            using var context = await _contextFactory.CreateDbContextAsync();
            var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == assignment.TaskId);
            var member = await context.Members.FirstOrDefaultAsync(m => m.Id == assignment.MemberId);

            if (task == null || member == null)
            {
                return NotFound("Task or member not found");
            }

            var assignmentDto = new TaskAssignmentDto
            {
                Id = assignment.Id,
                TaskId = assignment.TaskId,
                TaskName = task.TaskName,
                MemberId = assignment.MemberId,
                Host = member.Host,
                SlackId = member.SlackId
            };
            return Ok(assignmentDto);
        }
        catch (Exception ex)
        {
            return HandleException<TaskAssignmentDto>(ex);
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