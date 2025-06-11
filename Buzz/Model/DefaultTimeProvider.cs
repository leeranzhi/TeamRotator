namespace Buzz.Model;

public class DefaultTimeProvider : ITimeProvider
{
    public DateOnly GetCurrentDate()
    {
        return DateOnly.FromDateTime(DateTime.Now);
    }
}
