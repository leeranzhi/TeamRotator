using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class Member
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Host { get; set; } = string.Empty;
    
    [Required]
    public string SlackId { get; set; } = string.Empty;
} 