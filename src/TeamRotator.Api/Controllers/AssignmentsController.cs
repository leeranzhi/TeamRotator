using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using TeamRotator.Core.Entities;

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
    public async Task<ActionResult<List<TaskAssignmentDto>>> GetRotationList()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var assignments = await context.TaskAssignments
                .Include(ta => ta.Task)
                .Include(ta => ta.Member)
                .ToListAsync();

            var assignmentDtos = assignments.Select(a => new TaskAssignmentDto
            {
                Id = a.Id,
                TaskId = a.TaskId,
                MemberId = a.MemberId,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Task = a.Task ?? throw new InvalidOperationException($"Task not found for assignment {a.Id}"),
                Member = a.Member ?? throw new InvalidOperationException($"Member not found for assignment {a.Id}")
            }).ToList();

            return Ok(assignmentDtos);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var assignment = await context.TaskAssignments.FindAsync(id);
            
            if (assignment == null)
            {
                return NotFound($"Assignment with id {id} not found");
            }

            assignment.TaskId = modifyAssignmentDto.TaskId;
            assignment.MemberId = modifyAssignmentDto.MemberId;
            assignment.StartDate = modifyAssignmentDto.StartDate;
            assignment.EndDate = modifyAssignmentDto.EndDate;

            await context.SaveChangesAsync();
            
            var updatedAssignment = await context.TaskAssignments
                .Include(ta => ta.Task)
                .Include(ta => ta.Member)
                .FirstOrDefaultAsync(ta => ta.Id == id);

            if (updatedAssignment == null)
            {
                return NotFound("Updated assignment not found");
            }

            var assignmentDto = new TaskAssignmentDto
            {
                Id = updatedAssignment.Id,
                TaskId = updatedAssignment.TaskId,
                MemberId = updatedAssignment.MemberId,
                StartDate = updatedAssignment.StartDate,
                EndDate = updatedAssignment.EndDate,
                Task = updatedAssignment.Task ?? throw new InvalidOperationException($"Task not found for assignment {updatedAssignment.Id}"),
                Member = updatedAssignment.Member ?? throw new InvalidOperationException($"Member not found for assignment {updatedAssignment.Id}")
            };
            return Ok(assignmentDto);
        }
        catch (Exception ex)
        {
            return HandleException<TaskAssignmentDto>(ex);
        }
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