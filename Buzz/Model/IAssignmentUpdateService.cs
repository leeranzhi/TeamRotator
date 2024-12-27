using Buzz.Dto;

namespace Buzz.Model;

public interface IAssignmentUpdateService
{
    void UpdateTaskAssignment(TaskAssignment assignment);

    TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto); // 添加此方法声明
}
