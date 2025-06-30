using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class Task
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string TaskName { get; set; } = string.Empty;
    
    [Required]
    public string RotationRule { get; set; } = string.Empty;
} 