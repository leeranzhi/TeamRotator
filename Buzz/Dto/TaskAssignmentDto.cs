namespace Buzz.Dto;

public class TaskAssignmentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public int MemberId { get; set; }
    public string Host { get; set; }
    public string SlackId { get; set; }
}
