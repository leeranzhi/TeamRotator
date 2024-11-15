using Quartz;
using Buzz.Services;

namespace Buzz.Jobs;
public class AssignmentUpdateJob : IJob
{
    private readonly RotationService _assignmentUpdate;

    public AssignmentUpdateJob(RotationService assignmentUpdate)
    {
        _assignmentUpdate = assignmentUpdate;
    }

    public System.Threading.Tasks.Task Execute(IJobExecutionContext context)
    {
        _assignmentUpdate.UpdateTaskAssignmentList();
        return System.Threading.Tasks.Task.CompletedTask;
    }
}