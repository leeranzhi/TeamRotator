using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class Member
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string SlackId { get; set; }
} 