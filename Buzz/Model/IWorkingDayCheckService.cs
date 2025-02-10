namespace Buzz.Model;

public interface IWorkingDayCheckService
{
    Task<bool> IsWorkingDayCheck(DateTime date);
}
