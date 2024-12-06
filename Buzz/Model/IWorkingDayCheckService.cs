namespace Buzz.Model;

public interface IWorkingDayCheckService
{
    Task<bool> IsWorkingDay(DateTime date);
}
