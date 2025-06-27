using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;
using RotationTask = TeamRotator.Core.Entities.Task;

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
                (combined, task) => new
                {
                    combined.taskAssignment.Id,
                    combined.taskAssignment.TaskId,
                    TaskName = task.TaskName,
                    MemberId = combined.member.Id,
                    Host = combined.member.Host,
                    SlackId = combined.member.SlackId
                })
            .OrderBy(x => x.Id)
            .AsEnumerable()
            .Select(x => new TaskAssignmentDto
            {
                Id = x.Id,
                TaskId = x.TaskId,
                TaskName = x.TaskName ?? throw new InvalidOperationException($"Task name is null for task {x.TaskId}"),
                MemberId = x.MemberId,
                Host = x.Host ?? throw new InvalidOperationException($"Host is null for member {x.MemberId}"),
                SlackId = x.SlackId ?? throw new InvalidOperationException($"SlackId is null for member {x.MemberId}")
            })
            .ToList();

        _logger.LogInformation("Successfully fetched {RotationListCount} task assignments.", rotationList.Count);

        return rotationList;
    }

    private bool ShouldUpdateAssignment(TaskAssignment assignment)
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
        bool shouldUpdate = !(currentDate >= assignment.StartDate && currentDate <= assignment.EndDate);

        if (shouldUpdate)
        {
            _logger.LogInformation("Assignment ID {AssignmentId} needs to be updated. Current date: {CurrentDate}, Assignment Period: {StartDate} - {EndDate}",
                assignment.Id, currentDate, assignment.StartDate, assignment.EndDate);
        }

        return shouldUpdate;
    }

    public async Task UpdateTaskAssignmentList()
    {
        _logger.LogInformation("Starting updating all task assignment...");

        using var context = _contextFactory.CreateDbContext();

        foreach (var taskAssignment in context.TaskAssignments)
        {
            if (ShouldUpdateAssignment(taskAssignment))
            {
                _logger.LogInformation("Updating assignment ID {AssignmentId} for Task ID {TaskId}", taskAssignment.Id, taskAssignment.TaskId);
                await _assignmentUpdateService.UpdateTaskAssignment(taskAssignment);
            }
            else
            {
                _logger.LogInformation("Skipping update for assignment ID {AssignmentId}. Not within the valid date range.", taskAssignment.Id);
            }
        }
    }
} 