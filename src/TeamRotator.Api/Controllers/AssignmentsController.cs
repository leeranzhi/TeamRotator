using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using TeamRotator.Core.Entities;
using TeamRotator.Infrastructure.Services;

namespace TeamRotator.Api.Controllers;

[ApiController]
public class AssignmentsController : BaseController
{
    private readonly IRotationService _rotationService;
    private readonly IAssignmentUpdateService _assignmentUpdateService;
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly SendToSlackService _sendToSlackService;

    public AssignmentsController(
        ILogger<AssignmentsController> logger,
        IRotationService rotationService,
        IAssignmentUpdateService assignmentUpdateService,
        IDbContextFactory<RotationDbContext> contextFactory,
        SendToSlackService sendToSlackService)
        : base(logger)
    {
        _rotationService = rotationService;
        _assignmentUpdateService = assignmentUpdateService;
        _contextFactory = contextFactory;
        _sendToSlackService = sendToSlackService;
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

    [HttpPost("assign")]
    public async Task<ActionResult<TaskAssignment>> AssignTask([FromBody] AssignTaskDto dto)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var task = await context.Tasks.FindAsync(dto.TaskId);
            if (task == null)
            {
                return NotFound("Task not found");
            }

            var member = await context.Members.FindAsync(dto.MemberId);
            if (member == null)
            {
                return NotFound("Member not found");
            }

            var assignment = new TaskAssignment
            {
                TaskId = dto.TaskId,
                MemberId = dto.MemberId,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            context.TaskAssignments.Add(assignment);
            await context.SaveChangesAsync();

            return Ok(assignment);
        }
        catch (Exception ex)
        {
            return HandleException<TaskAssignment>(ex);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<TaskAssignment> UpdateRotationList(int id, [FromBody] ModifyAssignmentDto modifyAssignmentDto)
    {
        try
        {
            var updatedAssignment = _assignmentUpdateService.ModifyTaskAssignment(id, modifyAssignmentDto);
            return Ok(updatedAssignment);
        }
        catch (Exception ex)
        {
            return HandleException<TaskAssignment>(ex);
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

    [HttpPost("send-to-slack")]
    public async Task<IActionResult> SendToSlack()
    {
        try
        {
            await _sendToSlackService.SendSlackMessage();
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
} 