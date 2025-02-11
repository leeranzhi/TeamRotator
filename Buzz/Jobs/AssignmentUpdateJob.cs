using Quartz;
using Buzz.Model;
using Task = System.Threading.Tasks.Task;

namespace Buzz.Jobs;

public class AssignmentUpdateJob : IJob
{
    private readonly IRotationService _rotationService;

    public AssignmentUpdateJob(IRotationService rotationService)
    {
        _rotationService = rotationService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _rotationService.UpdateTaskAssignmentList();
        return Task.CompletedTask;
    }
}