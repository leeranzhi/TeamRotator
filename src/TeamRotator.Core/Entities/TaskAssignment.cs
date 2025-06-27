using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamRotator.Core.Entities;

public class TaskAssignment
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Task))]
    public int TaskId { get; set; }
    public virtual Task? Task { get; set; }

    [ForeignKey(nameof(Member))]
    public int MemberId { get; set; }
    public virtual Member? Member { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
} 