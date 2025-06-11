using Buzz.Dto;
using Buzz.Model;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using Task = System.Threading.Tasks.Task;

namespace Buzz.Services;

public class AssignmentUpdateService(
    IDbContextFactory<RotationDbContext> contextFactory,
    SendToSlackService slackService,
    ILogger<AssignmentUpdateService> logger,
    IWorkingDayCheckService workingDayCheckService,
    ITimeProvider? timeProvider = null)
    : IAssignmentUpdateService
{
    private readonly ITimeProvider _timeProvider = timeProvider ?? new DefaultTimeProvider();

    public async Task UpdateTaskAssignment(TaskAssignment assignment)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        logger.LogInformation("Updating task assignment for AssignmentId {AssignmentId}", assignment.Id);
        
        try
        {
            using var context = contextFactory.CreateDbContext();
            var task = context.Tasks
                .Where(t => t.Id == assignment.TaskId)
                .Select(t => new { t.RotationRule })
                .FirstOrDefault();

            DateOnly today = _timeProvider.GetCurrentDate();

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

            int rotationCount = 0;

            while (ShouldRotateToday(task.RotationRule, currentAssignment.StartDate, currentAssignment.EndDate, today))
            {
                (DateOnly start, DateOnly end) = CalculateNextDateRange(task.RotationRule, currentAssignment.StartDate);
                currentAssignment.StartDate = start;
                currentAssignment.EndDate = end;

                if (task.RotationRule == "daily" &&
                    !await workingDayCheckService.IsWorkingDayCheck(start.ToDateTime(TimeOnly.MinValue)))
                {
                    logger.LogInformation("{Date} is not a working day. Skipping member rotation for AssignmentId {AssignmentId}", start, assignment.Id);
                    continue;
                }

                RotateMemberList(currentAssignment, context);
                rotationCount++;
            }

            if (rotationCount > 0)
            {
                context.SaveChanges();
                logger.LogInformation("Successfully updated AssignmentId {AssignmentId} after {RotationCount} rotation(s)", assignment.Id, rotationCount);
            }
            else
            {
                logger.LogInformation("No rotation needed for AssignmentId {AssignmentId} on {Today}", assignment.Id, today);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task assignment for AssignmentId {AssignmentId}", assignment.Id);
            slackService.SendFailedMessageToSlack($"Failed to update task assignment: {ex.Message}");
            throw;
        }
    }

    private bool ShouldRotateToday(string? rule, DateOnly? startDate, DateOnly? endDate, DateOnly today)
    {
        if (string.IsNullOrEmpty(rule)) return false;
        if (startDate == null || endDate == null) return true;
        return today > endDate;
    }
    private (DateOnly Start, DateOnly End) CalculateNextDateRange(string? rule, DateOnly fromDate)
    {
        if (rule == "daily")
        {
            var next = fromDate.AddDays(1);
            return (next, next);
        }

        var parts = rule?.Split('_');
        if (parts == null || parts.Length != 2)
            throw new InvalidOperationException($"Invalid rotation rule: {rule}");

        var frequency = parts[0];
        var dayOfWeekStr = parts[1];

        if (!Enum.TryParse<DayOfWeek>(Capitalize(dayOfWeekStr), out var targetDay))
            throw new InvalidOperationException($"Invalid day in rotation rule: {dayOfWeekStr}");
       
        DateOnly firstTargetDayAfter = GetNextDayAfterTargetDay(fromDate, targetDay);
        
        switch (frequency)
        {
            case "weekly":
                return (firstTargetDayAfter, firstTargetDayAfter.AddDays(6));

            case "biweekly":
                var secondTargetDay = GetNextDayAfterTargetDay(firstTargetDayAfter, targetDay);
                return (secondTargetDay, secondTargetDay.AddDays(13));

            default:
                throw new InvalidOperationException($"Unsupported frequency: {frequency}");
        }
    }
    
    private static DateOnly GetNextDayAfterTargetDay(DateOnly start, DayOfWeek targetDay)
    {
        int daysToAdd = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
        daysToAdd = daysToAdd == 0 ? 7 : daysToAdd;
        var targetDate = start.AddDays(daysToAdd);
        return targetDate.AddDays(1);
    }

    private void RotateMemberList(TaskAssignment assignment, RotationDbContext context)
    {
        var members = context.Members.OrderBy(m => m.Id).ToList();
        var currentIndex = members.FindIndex(m => m.Id == assignment.MemberId);
        if (currentIndex < 0) throw new InvalidOperationException("Current member not found.");

        var nextMember = members[(currentIndex + 1) % members.Count];
        assignment.MemberId = nextMember.Id;
    }

    public TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());

        using var context = contextFactory.CreateDbContext();
        var assignment = context.TaskAssignments.FirstOrDefault(a => a.Id == id)
                         ?? throw new InvalidOperationException("Assignment not found.");

        var member = context.Members.FirstOrDefault(m => m.Host == modifyAssignmentDto.Host)
                     ?? throw new InvalidOperationException("Member not found.");

        assignment.MemberId = member.Id;
        context.SaveChanges();

        logger.LogInformation("Successfully modified AssignmentId {AssignmentId} to MemberId {MemberId}", id, member.Id);
        return assignment;
    }

    private static string Capitalize(string input)
    {
        return char.ToUpperInvariant(input[0]) + input[1..].ToLowerInvariant();
    }
}
