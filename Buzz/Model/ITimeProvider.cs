namespace Buzz.Model;

public interface ITimeProvider
{
    DateOnly GetCurrentDate();
}
