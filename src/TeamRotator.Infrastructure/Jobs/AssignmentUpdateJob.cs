using Quartz;
using TeamRotator.Core.Interfaces;

namespace TeamRotator.Infrastructure.Jobs;

public class AssignmentUpdateJob : BaseJob
{
    private readonly IRotationService _rotationService;

    public AssignmentUpdateJob(
        ILogger<AssignmentUpdateJob> logger,
        IRotationService rotationService)
        : base(logger)
    {
        _rotationService = rotationService;
    }

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        await _rotationService.UpdateTaskAssignmentList();
    }
} 