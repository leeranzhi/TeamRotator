using System.ComponentModel.DataAnnotations;

namespace TeamRotator.Core.Entities;

public class Member
{
    [Key]
    public int Id { get; set; }
    public string? Host { get; set; }
    public string? SlackId { get; set; }
} 