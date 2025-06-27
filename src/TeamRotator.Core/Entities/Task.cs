using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class Task
{
    [Key]
    public int Id { get; set; }
    public string? TaskName { get; set; }
    public string? RotationRule { get; set; }
} 