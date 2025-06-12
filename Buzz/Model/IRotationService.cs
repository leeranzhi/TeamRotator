using Buzz.Dto;
using SystemTask = System.Threading.Tasks.Task;

namespace Buzz.Model;

public interface IRotationService
{
    List<TaskAssignmentDto> GetRotationList();
    
    SystemTask UpdateTaskAssignmentList();
}
