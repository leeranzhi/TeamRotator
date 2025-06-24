using Microsoft.Extensions.Logging;
using Quartz;
using TeamRotator.Core.Entities;
using TeamRotator.Core.Interfaces;

namespace TeamRotator.Infrastructure.Jobs;

public class AssignmentUpdateJob : BaseJob
{
    private readonly IAssignmentUpdateService _assignmentUpdateService;

    public AssignmentUpdateJob(
        IAssignmentUpdateService assignmentUpdateService,
        ILogger<AssignmentUpdateJob> logger) : base(logger)
    {
        _assignmentUpdateService = assignmentUpdateService;
    }

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting AssignmentUpdateJob at: {time}", DateTimeOffset.Now);
        try
        {
            await _assignmentUpdateService.UpdateTaskAssignment(new TaskAssignment
            {
                Id = 1, // This is just a placeholder, the actual implementation should get the current assignment
                TaskId = 1,
                MemberId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
            });
            
            _logger.LogInformation("AssignmentUpdateJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing AssignmentUpdateJob");
            throw;
        }
    }
} 