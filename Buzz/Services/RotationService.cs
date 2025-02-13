using Buzz.Dto;
using Buzz.Model;
using Microsoft.EntityFrameworkCore;

namespace Buzz.Services;

public class RotationService(IDbContextFactory<RotationDbContext> contextFactory,
        IAssignmentUpdateService assignmentUpdateService,
        ILogger<AssignmentUpdateService> logger)
    : IRotationService
{
    public List<TaskAssignmentDto> GetRotationList()
    {
        logger.LogInformation("Fetching rotation list...");

        using var context = contextFactory.CreateDbContext();

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
            .Select(x => new TaskAssignmentDto
            {
                Id = x.Id,
                TaskId = x.TaskId,
                TaskName = x.TaskName,
                MemberId = x.MemberId,
                Host = x.Host,
                SlackId = x.SlackId
            })
            .ToList();

        logger.LogInformation("Successfully fetched {RotationListCount} task assignments.", rotationList.Count);

        return rotationList;
    }

    private bool ShouldUpdateAssignment(TaskAssignment assignment)
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
        bool shouldUpdate = !(currentDate >= assignment.StartDate && currentDate <= assignment.EndDate);

        if (shouldUpdate)
        {
            logger.LogInformation("Assignment ID {AssignmentId} needs to be updated. Current date: {CurrentDate}, Assignment Period: {StartDate} - {EndDate}",
                assignment.Id, currentDate, assignment.StartDate, assignment.EndDate);
        }

        return shouldUpdate;
    }

    public void UpdateTaskAssignmentList()
    {
        logger.LogInformation("Starting updating all task assignment...");

        using var context = contextFactory.CreateDbContext();

        foreach (var taskAssignment in context.TaskAssignments)
        {
            if (ShouldUpdateAssignment(taskAssignment))
            {
                logger.LogInformation("Updating assignment ID {AssignmentId} for Task ID {TaskId}", taskAssignment.Id, taskAssignment.TaskId);
                assignmentUpdateService.UpdateTaskAssignment(taskAssignment);
            }
            else
            {
                logger.LogInformation("Skipping update for assignment ID {AssignmentId}. Not within the valid date range.", taskAssignment.Id);
            }
        }
    }
}
