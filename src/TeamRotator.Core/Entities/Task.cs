using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class RotationTask
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string RotationRule { get; set; }
} 