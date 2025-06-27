using TeamRotator.Core.Entities;

namespace TeamRotator.Core.DTOs;

public class TaskAssignmentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public required string TaskName { get; set; }
    public int MemberId { get; set; }
    public required string Host { get; set; }
    public required string SlackId { get; set; }
} 