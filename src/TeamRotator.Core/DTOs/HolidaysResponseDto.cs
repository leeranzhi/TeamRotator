namespace TeamRotator.Core.DTOs;

public class HolidaysResponseDto
{
    public int Year { get; set; }
    public required List<HolidayDto> Days { get; set; }
} 