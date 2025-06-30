namespace TeamRotator.Core.DTOs;

public class ModifyAssignmentDto
{
    public required string Host { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
} 