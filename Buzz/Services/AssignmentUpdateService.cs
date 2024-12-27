using Buzz.Dto;
using Buzz.Model;
using Microsoft.EntityFrameworkCore;

namespace Buzz.Services;

public class AssignmentUpdateService : IAssignmentUpdateService
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly SendToSlackService _slackService;
    private readonly ITimeProvider _timeProvider;

    public AssignmentUpdateService(IDbContextFactory<RotationDbContext> contextFactory, SendToSlackService slackService,
        ITimeProvider timeProvider = null)
    {
        _contextFactory = contextFactory;
        _slackService = slackService;
        _timeProvider = timeProvider ?? new DefaultTimeProvider();
    }

    public void UpdateTaskAssignment(TaskAssignment assignment)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var task = context.Tasks
                .Where(t => t.Id == assignment.TaskId)
                .Select(t => new { t.PeriodType })
                .FirstOrDefault();

            DateOnly currentDate = _timeProvider.GetCurrentDate();

            if (task == null)
            {
                throw new InvalidOperationException("Task not found.");
            }

            var currentAssignment = context.TaskAssignments
                .FirstOrDefault(a => a.Id == assignment.Id);

            if (currentAssignment == null)
            {
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
                    DateOnly nextSunday = lastMonday.AddDays(6);
                    currentAssignment.StartDate = lastMonday;
                    currentAssignment.EndDate = nextSunday;
                    break;

                case "fortnightly":
                    DateOnly lastWednesday = GetLastWednesday(currentDate);
                    currentAssignment.StartDate = lastWednesday;
                    currentAssignment.EndDate = lastWednesday.AddDays(13);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported PeriodType: {task.PeriodType}");
            }

            RotateMemberList(currentAssignment, context);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating task assignment: {ex.Message}");
            _slackService.SendFailedMessageToSlack($"Failed to update task assignment: {ex.Message}");
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
            Console.WriteLine($"An error occurred while rotating the member list: {ex.Message}");
            throw;
        }
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

    public TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto)
    {
        using var context = _contextFactory.CreateDbContext();

        var modifiedAssignment = context.TaskAssignments
            .FirstOrDefault(a => a.Id == id);

        if (modifiedAssignment == null)
        {
            throw new InvalidOperationException("Assignment not found.");
        }

        var member = context.Members
            .FirstOrDefault(m => m.Host == modifyAssignmentDto.Host);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found.");
        }

        modifiedAssignment.MemberId = member.Id;

        context.SaveChanges();

        return modifiedAssignment;
    }
}
