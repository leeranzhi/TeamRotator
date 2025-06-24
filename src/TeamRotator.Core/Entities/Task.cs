using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class RotationTask
{
    [Key]
    public int Id { get; set; }
    public required string TaskName { get; set; }
    public required string RotationRule { get; set; }
} 