using TeamRotator.Core.Entities;

namespace TeamRotator.Core.DTOs;

public class TaskAssignmentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int MemberId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public required RotationTask Task { get; set; }
    public required Member Member { get; set; }
} 