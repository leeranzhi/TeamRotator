using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Infrastructure.Services;

public class RotationService : IRotationService
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly IAssignmentUpdateService _assignmentUpdateService;
    private readonly ILogger<RotationService> _logger;

    public RotationService(
        IDbContextFactory<RotationDbContext> contextFactory,
        IAssignmentUpdateService assignmentUpdateService,
        ILogger<RotationService> logger)
    {
        _contextFactory = contextFactory;
        _assignmentUpdateService = assignmentUpdateService;
        _logger = logger;
    }

    public List<TaskAssignmentDto> GetRotationList()
    {
        _logger.LogInformation("Fetching rotation list...");

        using var context = _contextFactory.CreateDbContext();

        var rotationList = context.TaskAssignments
            .Join(context.Members,
                taskAssignment => taskAssignment.MemberId,
                member => member.Id,
                (taskAssignment, member) => new { taskAssignment, member })
            .Join(context.Tasks,
                combined => combined.taskAssignment.TaskId,
                task => task.Id,
                (combined, task) => new TaskAssignmentDto
                {
                    Id = combined.taskAssignment.Id,
                    TaskId = combined.taskAssignment.TaskId,
                    TaskName = task.TaskName,
                    MemberId = combined.member.Id,
                    Host = combined.member.Host,
                    SlackId = combined.member.SlackId
                })
            .OrderBy(x => x.Id)
            .ToList();

        _logger.LogInformation("Successfully fetched {Count} task assignments", rotationList.Count);
        return rotationList;
    }

    private bool ShouldUpdateAssignment(Core.Entities.TaskAssignment assignment)
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
        bool shouldUpdate = !(currentDate >= assignment.StartDate && currentDate <= assignment.EndDate);

        if (shouldUpdate)
        {
            _logger.LogInformation(
                "Assignment ID {AssignmentId} needs to be updated. Current date: {CurrentDate}, Assignment Period: {StartDate} - {EndDate}",
                assignment.Id, currentDate, assignment.StartDate, assignment.EndDate);
        }

        return shouldUpdate;
    }

    public async Task UpdateTaskAssignmentList()
    {
        _logger.LogInformation("Starting to update all task assignments...");

        using var context = _contextFactory.CreateDbContext();

        foreach (var taskAssignment in context.TaskAssignments)
        {
            if (ShouldUpdateAssignment(taskAssignment))
            {
                _logger.LogInformation("Updating assignment ID {AssignmentId} for Task ID {TaskId}",
                    taskAssignment.Id, taskAssignment.TaskId);
                await _assignmentUpdateService.UpdateTaskAssignment(taskAssignment);
            }
            else
            {
                _logger.LogInformation("Skipping update for assignment ID {AssignmentId}. Not within the valid date range.",
                    taskAssignment.Id);
            }
        }
    }
} 