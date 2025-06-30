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

        var assignments = context.TaskAssignments
            .Include(ta => ta.Task)
            .Include(ta => ta.Member)
            .OrderBy(x => x.Id)
            .AsEnumerable()
            .Select(ta => new TaskAssignmentDto
            {
                Id = ta.Id,
                TaskId = ta.TaskId,
                TaskName = ta.Task?.TaskName ?? throw new InvalidOperationException($"Task name is null for task {ta.TaskId}"),
                MemberId = ta.MemberId,
                Host = ta.Member?.Host ?? throw new InvalidOperationException($"Host is null for member {ta.MemberId}"),
                SlackId = ta.Member?.SlackId ?? throw new InvalidOperationException($"SlackId is null for member {ta.MemberId}"),
                StartDate = ta.StartDate.ToString("yyyy-MM-dd"),
                EndDate = ta.EndDate.ToString("yyyy-MM-dd")
            })
            .ToList();

        _logger.LogInformation("Successfully fetched {RotationListCount} task assignments.", assignments.Count);

        return assignments;
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