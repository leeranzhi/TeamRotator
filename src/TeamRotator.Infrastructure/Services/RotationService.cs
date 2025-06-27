using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Infrastructure.Services;

public class RotationService : IRotationService
{
    private readonly ILogger<RotationService> _logger;
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;
    private readonly IWorkingDayCheckService _workingDayCheckService;

    public RotationService(
        ILogger<RotationService> logger,
        IDbContextFactory<RotationDbContext> contextFactory,
        IWorkingDayCheckService workingDayCheckService)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _workingDayCheckService = workingDayCheckService;
    }

    public List<TaskAssignmentDto> GetRotationList()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var assignments = context.TaskAssignments
                .Include(ta => ta.Task)
                .Include(ta => ta.Member)
                .ToList();

            return assignments.Select(a => new TaskAssignmentDto
            {
                Id = a.Id,
                TaskId = a.TaskId,
                MemberId = a.MemberId,
                Task = a.Task ?? throw new InvalidOperationException($"Task not found for assignment {a.Id}"),
                Member = a.Member ?? throw new InvalidOperationException($"Member not found for assignment {a.Id}")
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rotation list");
            throw;
        }
    }

    public async Task UpdateTaskAssignmentList()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tasks = await context.Tasks.ToListAsync();
            var members = await context.Members.ToListAsync();

            if (!members.Any())
            {
                _logger.LogWarning("No members found to assign tasks to");
                return;
            }

            foreach (var task in tasks)
            {
                var currentAssignment = await context.TaskAssignments
                    .Include(ta => ta.Member)
                    .FirstOrDefaultAsync(ta => ta.TaskId == task.Id);

                if (currentAssignment == null)
                {
                    // If no assignment exists, create one with the first member
                    var firstMember = members.First();
                    context.TaskAssignments.Add(new TaskAssignment
                    {
                        TaskId = task.Id,
                        MemberId = firstMember.Id
                    });
                }
                else
                {
                    // Get the next member in rotation
                    var currentMemberIndex = members.FindIndex(m => m.Id == currentAssignment.MemberId);
                    var nextMemberIndex = (currentMemberIndex + 1) % members.Count;
                    var nextMember = members[nextMemberIndex];

                    currentAssignment.MemberId = nextMember.Id;
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task assignment list");
            throw;
        }
    }
} 