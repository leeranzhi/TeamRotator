namespace Buzz.Model;

public class TestTimeProvider : ITimeProvider
{
    private readonly DateOnly _currentDate;

    public TestTimeProvider(DateOnly currentDate)
    {
        _currentDate = currentDate;
    }

    public DateOnly GetCurrentDate()
    {
        return _currentDate;
    }
}
