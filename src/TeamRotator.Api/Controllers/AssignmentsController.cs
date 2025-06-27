using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using TeamRotator.Core.Entities;

namespace TeamRotator.Api.Controllers;

[ApiController]
[Route("assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly IRotationService _rotationService;
    private readonly IAssignmentUpdateService _assignmentUpdateService;
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;

    public AssignmentsController(
        IRotationService rotationService,
        IAssignmentUpdateService assignmentUpdateService,
        IDbContextFactory<RotationDbContext> contextFactory)
    {
        _rotationService = rotationService;
        _assignmentUpdateService = assignmentUpdateService;
        _contextFactory = contextFactory;
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

    [HttpPost]
    public async Task<ActionResult<TaskAssignmentDto>> CreateAssignment([FromBody] ModifyAssignmentDto createAssignmentDto)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var task = await context.Tasks.FindAsync(createAssignmentDto.TaskId);
            var member = await context.Members.FindAsync(createAssignmentDto.MemberId);

            if (task == null || member == null)
            {
                return NotFound("Task or member not found");
            }

            var assignment = new TaskAssignment
            {
                TaskId = createAssignmentDto.TaskId,
                MemberId = createAssignmentDto.MemberId,
                StartDate = createAssignmentDto.StartDate,
                EndDate = createAssignmentDto.EndDate
            };

            context.TaskAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var assignmentDto = new TaskAssignmentDto
            {
                Id = assignment.Id,
                TaskId = assignment.TaskId,
                MemberId = assignment.MemberId,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Task = task,
                Member = member
            };

            return CreatedAtAction(nameof(GetRotationList), new { id = assignment.Id }, assignmentDto);
        }
        catch (Exception ex)
        {
            return HandleException<TaskAssignmentDto>(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var assignment = await context.TaskAssignments.FindAsync(id);
            
            if (assignment == null)
            {
                return NotFound($"Assignment with id {id} not found");
            }

            context.TaskAssignments.Remove(assignment);
            await context.SaveChangesAsync();

            return NoContent();
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