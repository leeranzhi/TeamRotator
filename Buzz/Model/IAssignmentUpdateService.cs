using Buzz.Dto;
using SystemTask = System.Threading.Tasks.Task;

namespace Buzz.Model;

public interface IAssignmentUpdateService
{
    SystemTask UpdateTaskAssignment(TaskAssignment assignment);
    
    TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto);
}
