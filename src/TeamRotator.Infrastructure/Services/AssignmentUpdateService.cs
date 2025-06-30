using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace TeamRotator.Infrastructure.Services;

public class AssignmentUpdateService : IAssignmentUpdateService
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly SendToSlackService _slackService;
    private readonly ILogger<AssignmentUpdateService> _logger;
    private readonly IWorkingDayCheckService _workingDayCheckService;
    private readonly ITimeProvider _timeProvider;

    public AssignmentUpdateService(
        IDbContextFactory<RotationDbContext> contextFactory,
        SendToSlackService slackService,
        ILogger<AssignmentUpdateService> logger,
        IWorkingDayCheckService workingDayCheckService,
        ITimeProvider timeProvider)
    {
        _contextFactory = contextFactory;
        _slackService = slackService;
        _logger = logger;
        _workingDayCheckService = workingDayCheckService;
        _timeProvider = timeProvider;
    }

    public async Task UpdateTaskAssignment(TaskAssignment assignment)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        _logger.LogInformation("Updating task assignment for AssignmentId {AssignmentId}", assignment.Id);
        
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var task = context.Tasks
                .Where(t => t.Id == assignment.TaskId)
                .Select(t => new { t.RotationRule })
                .FirstOrDefault();

            DateOnly today = _timeProvider.GetCurrentDate();

            if (task == null)
            {
                _logger.LogWarning("Task not found for TaskId {TaskId}", assignment.TaskId);
                throw new InvalidOperationException("Task not found.");
            }

            var currentAssignment = context.TaskAssignments
                .FirstOrDefault(a => a.Id == assignment.Id);

            if (currentAssignment == null)
            {
                _logger.LogWarning("Assignment not found for AssignmentId {AssignmentId}", assignment.Id);
                throw new InvalidOperationException("Assignment not found.");
            }

            int rotationCount = 0;

            while (ShouldRotateToday(task.RotationRule, currentAssignment.StartDate, currentAssignment.EndDate, today))
            {
                (DateOnly start, DateOnly end) = CalculateNextDateRange(task.RotationRule, currentAssignment.StartDate);
                currentAssignment.StartDate = start;
                currentAssignment.EndDate = end;

                if (task.RotationRule == "daily" &&
                    !await _workingDayCheckService.IsWorkingDayCheck(start.ToDateTime(TimeOnly.MinValue)))
                {
                    _logger.LogInformation("{Date} is not a working day. Skipping member rotation for AssignmentId {AssignmentId}",
                        start, assignment.Id);
                    continue;
                }

                RotateMemberList(currentAssignment, context);
                rotationCount++;
            }

            if (rotationCount > 0)
            {
                context.SaveChanges();
                _logger.LogInformation("Successfully updated AssignmentId {AssignmentId} after {RotationCount} rotation(s)",
                    assignment.Id, rotationCount);
            }
            else
            {
                _logger.LogInformation("No rotation needed for AssignmentId {AssignmentId} on {Today}",
                    assignment.Id, today);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task assignment for AssignmentId {AssignmentId}", assignment.Id);
            await _slackService.SendFailedMessageToSlack($"Failed to update task assignment: {ex.Message}");
            throw;
        }
    }

    public TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());

        using var context = _contextFactory.CreateDbContext();
        var assignment = context.TaskAssignments.FirstOrDefault(a => a.Id == id)
                         ?? throw new InvalidOperationException("Assignment not found.");

        var member = context.Members.FirstOrDefault(m => m.Host == modifyAssignmentDto.Host)
                     ?? throw new InvalidOperationException("Member not found.");

        assignment.MemberId = member.Id;
        assignment.StartDate = modifyAssignmentDto.StartDate;
        assignment.EndDate = modifyAssignmentDto.EndDate;

        context.SaveChanges();

        _logger.LogInformation(
            "Successfully modified AssignmentId {AssignmentId} to MemberId {MemberId} with date range {StartDate} - {EndDate}",
            id, member.Id, modifyAssignmentDto.StartDate, modifyAssignmentDto.EndDate);

        return assignment;
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

    private static void RotateMemberList(TaskAssignment assignment, RotationDbContext context)
    {
        var members = context.Members.OrderBy(m => m.Id).ToList();
        var currentIndex = members.FindIndex(m => m.Id == assignment.MemberId);
        if (currentIndex < 0) throw new InvalidOperationException("Current member not found.");

        var nextMember = members[(currentIndex + 1) % members.Count];
        assignment.MemberId = nextMember.Id;
    }

    private static string Capitalize(string input)
    {
        return char.ToUpperInvariant(input[0]) + input[1..].ToLowerInvariant();
    }
} 