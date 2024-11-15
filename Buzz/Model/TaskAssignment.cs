using System.ComponentModel.DataAnnotations;

namespace Buzz.Model;

public class TaskAssignment
{
    [Key]public int Id { get; set; }
    public int TaskId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int MemberId { get; set; }
}