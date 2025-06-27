using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.DTOs;

public class AssignTaskDto
{
    [Required]
    public int TaskId { get; set; }

    [Required]
    public int MemberId { get; set; }
} 