using Buzz.Dto;
using Buzz.Model;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;

namespace Buzz.Services;

public class AssignmentUpdateService(IDbContextFactory<RotationDbContext> contextFactory,
        SendToSlackService slackService,
        ILogger<AssignmentUpdateService> logger, ITimeProvider? timeProvider = null)
    : IAssignmentUpdateService
{
    private readonly ITimeProvider _timeProvider = timeProvider ?? new DefaultTimeProvider();

    public void UpdateTaskAssignment(TaskAssignment assignment)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        logger.LogInformation("Updating task assignment for AssignmentId {AssignmentId}", assignment.Id);
        
        try
        {
            using var context = contextFactory.CreateDbContext();
            var task = context.Tasks
                .Where(t => t.Id == assignment.TaskId)
                .Select(t => new { t.PeriodType })
                .FirstOrDefault();

            DateOnly currentDate = _timeProvider.GetCurrentDate();

            if (task == null)
            {
                logger.LogWarning("Task not found for TaskId {TaskId}", assignment.TaskId);
                throw new InvalidOperationException("Task not found.");
            }

            var currentAssignment = context.TaskAssignments
                .FirstOrDefault(a => a.Id == assignment.Id);

            if (currentAssignment == null)
            {
                logger.LogWarning("Assignment not found for AssignmentId {AssignmentId}", assignment.Id);
                throw new InvalidOperationException("Assignment not found.");
            }

            switch (task.PeriodType)
            {
                case "daily":
                    currentAssignment.StartDate = currentDate;
                    currentAssignment.EndDate = currentDate;
                    break;

                case "weekly":
                    DateOnly lastMonday = GetLastMonday(currentDate);
                    currentAssignment.StartDate = lastMonday;
                    currentAssignment.EndDate = lastMonday.AddDays(6);
                    break;

                case "fortnightly":
                    DateOnly lastWednesday = GetLastWednesday(currentDate);
                    currentAssignment.StartDate = lastWednesday;
                    currentAssignment.EndDate = lastWednesday.AddDays(13);
                    break;
                default:
                    logger.LogError("Unsupported PeriodType {PeriodType} for TaskId {TaskId}", task.PeriodType, assignment.TaskId);
                    throw new InvalidOperationException($"Unsupported PeriodType: {task.PeriodType}");
            }

            RotateMemberList(currentAssignment, context);
            context.SaveChanges();
            logger.LogInformation("Successfully updated AssignmentId {AssignmentId}", assignment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task assignment for AssignmentId {AssignmentId}", assignment.Id);
            slackService.SendFailedMessageToSlack($"Failed to update task assignment: {ex.Message}");
            throw;
        }
    }

    private void RotateMemberList(TaskAssignment assignment, RotationDbContext context)
    {
        try
        {
            var members = context.Members.OrderBy(m => m.Id).ToList();
            var currentMemberIndex = members.FindIndex(m => m.Id == assignment.MemberId);
            if (currentMemberIndex < 0)
            {
                throw new InvalidOperationException("Current member not found.");
            }

            var nextMemberIndex = (currentMemberIndex + 1) % members.Count;
            var nextMember = members[nextMemberIndex];
            assignment.MemberId = nextMember.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rotating member list for AssignmentId {AssignmentId}", assignment.Id);
            throw;
        }
    }

    public TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        
        using var context = contextFactory.CreateDbContext();
        var modifiedAssignment = context.TaskAssignments.FirstOrDefault(a => a.Id == id);

        if (modifiedAssignment == null)
        {
            logger.LogWarning("Assignment not found for AssignmentId {AssignmentId}", id);
            throw new InvalidOperationException("Assignment not found.");
        }

        var member = context.Members
            .FirstOrDefault(m => m.Host == modifyAssignmentDto.Host);

        if (member == null)
        {
            logger.LogWarning("Member not found for Host {Host}", modifyAssignmentDto.Host);
            throw new InvalidOperationException("Member not found.");
        }

        modifiedAssignment.MemberId = member.Id;
        context.SaveChanges();
        
        logger.LogInformation("Successfully modified AssignmentId {AssignmentId} to MemberId {MemberId}", id, member.Id);
        return modifiedAssignment;
    }

    private DateOnly GetLastMonday(DateOnly date)
    {
        int daysBack = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysBack < 0) daysBack += 7;
        return date.AddDays(-daysBack);
    }

    private DateOnly GetLastWednesday(DateOnly date)
    {
        int daysBack = (int)date.DayOfWeek - (int)DayOfWeek.Wednesday;
        if (daysBack < 0) daysBack += 7;
        return date.AddDays(-daysBack);
    }
}
