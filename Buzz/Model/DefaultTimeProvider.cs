namespace Buzz.Model;

public class DefaultTimeProvider : ITimeProvider
{
    private readonly DateOnly _currentDate;

    public DateOnly GetCurrentDate()
    {
        return DateOnly.FromDateTime(DateTime.Now);
    }
}