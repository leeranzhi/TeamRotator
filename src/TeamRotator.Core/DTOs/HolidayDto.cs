namespace TeamRotator.Core.DTOs;

public class HolidayDto
{
    public required string Name { get; set; }
    public required string Date { get; set; }
    public bool IsOffDay { get; set; }
} 