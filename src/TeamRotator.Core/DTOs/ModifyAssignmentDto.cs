namespace TeamRotator.Core.DTOs;

public class ModifyAssignmentDto
{
    public int TaskId { get; set; }
    public int MemberId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
} 