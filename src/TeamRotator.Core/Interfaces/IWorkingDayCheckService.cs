namespace TeamRotator.Core.Interfaces;

public interface IWorkingDayCheckService
{
    Task<bool> IsWorkingDayCheck(DateTime date);
} 