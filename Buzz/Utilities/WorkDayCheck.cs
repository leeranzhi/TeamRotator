using ChineseCalendar;

namespace Buzz.Utilities;

public class WorkingDayCheck
{
    private static List<Festival> GetFestivals()
    {
        IEnumerable<Festival> allDefined = Festival.GetAllDefined() ?? throw new InvalidOperationException("Failed to retrieve festivals.");
        return allDefined.ToList();
    }

    private static List<DateTime> GetGregorianHolidays(List<Festival> festivals, int year)
    {
        List<DateTime> gregorianHolidays = new List<DateTime>();

        foreach (var festival in festivals)
        {
            if (festival is GregorianFestival)
            {
                gregorianHolidays.Add(new DateTime(year, festival.Month, festival.Day));
            }
        }

        return gregorianHolidays;
    }

    private static bool IsHoliday(DateTime date, List<DateTime> gregorianHolidays)
    {
        return gregorianHolidays.Contains(date.Date);
    }

    private static bool IsWeekday(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    public static bool IsWorkingDay(DateTime date)
    {
        List<Festival> festivals = GetFestivals();
        List<DateTime> gregorianHolidays = GetGregorianHolidays(festivals, date.Year);
        return !IsHoliday(date, gregorianHolidays) && IsWeekday(date);
    }
}
