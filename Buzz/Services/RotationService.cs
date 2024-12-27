using Buzz.Model;
using Microsoft.EntityFrameworkCore;

namespace Buzz.Services;

public class RotationService : IRotationService
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly IAssignmentUpdateService _assignmentUpdateService;

    public RotationService(IDbContextFactory<RotationDbContext> contextFactory, IAssignmentUpdateService assignmentUpdateService)
    {
        _contextFactory = contextFactory;
        _assignmentUpdateService = assignmentUpdateService;
    }
    
    public List<object> GetRotationList()
    {
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
            .ToList();

        return rotationList.Cast<object>().ToList();
    }

    private bool ShouldUpdateAssignment(TaskAssignment assignment)
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
        return !(currentDate >= assignment.StartDate && currentDate <= assignment.EndDate);
    }

    public void UpdateTaskAssignmentList()
    {
        using var context = _contextFactory.CreateDbContext();

        foreach (var taskAssignment in context.TaskAssignments)
        {
            if (ShouldUpdateAssignment(taskAssignment))
            {
                _assignmentUpdateService.UpdateTaskAssignment(taskAssignment);
            }
        }
    }
}
