using System.Globalization;
using Buzz.Dto;
using Newtonsoft.Json;

namespace Buzz.Services;

public class WorkingDayCheckService
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private string _url = "https://raw.githubusercontent.com/NateScarlet/holiday-cn/master/2024.json";

    public async Task<bool> IsWorkingDay(DateTime currentDate)
    {
        var holidays = await GetHolidays(_url);
        var holiday = holidays.Find(h => h.Date == currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        
        if (holiday != null) 
        {
            return !holiday.IsOffDay;
        }

        return !(currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday);
    }

    private async Task<List<HolidayDto>> GetHolidays(string url)
    {
        try
        {
            HttpResponseMessage response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();

            HolidaysResponse? holidays = JsonConvert.DeserializeObject<HolidaysResponse>(jsonContent);

            return holidays?.Days ?? new List<HolidayDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching JSON data: {ex.Message}");
            return new List<HolidayDto>();
        }
    }
}