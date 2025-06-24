using System.Threading.Tasks;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Entities;

namespace TeamRotator.Core.Interfaces;

public interface IAssignmentUpdateService
{
    System.Threading.Tasks.Task UpdateTaskAssignment(TaskAssignment assignment);
    TaskAssignment ModifyTaskAssignment(int id, ModifyAssignmentDto modifyAssignmentDto);
} 