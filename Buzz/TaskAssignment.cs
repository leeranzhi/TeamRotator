using System.ComponentModel.DataAnnotations;

namespace Buzz;

public class TaskAssignment
{
    [Key]public int Id { get; set; }
    public int TaskId { get; set; }
    public string? PersonName { get; set; }
    public string? PersonId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MemberId { get; set; }
}