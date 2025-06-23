using TeamRotator.Core.DTOs;

namespace TeamRotator.Core.Interfaces;

public interface IRotationService
{
    List<TaskAssignmentDto> GetRotationList();
    Task UpdateTaskAssignmentList();
} 