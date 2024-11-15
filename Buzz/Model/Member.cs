using System.ComponentModel.DataAnnotations;

namespace Buzz.Model;

public class Member
{
    [Key]public int Id { get; set; }
    public string? Host { get; set; }
    public string? SlackId { get; set; }
}