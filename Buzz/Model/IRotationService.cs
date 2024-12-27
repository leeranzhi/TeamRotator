using Buzz.Dto;

namespace Buzz.Model;

public interface IRotationService
{
    List<TaskAssignmentDto> GetRotationList();
}