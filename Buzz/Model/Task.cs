using System.ComponentModel.DataAnnotations;

namespace Buzz.Model;

public class Task
{
    [Key] public int Id { get; set; }
    public string? TaskName { get; set; }
    public string? PeriodType { get; set; }
}