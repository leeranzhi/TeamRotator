using System.Globalization;
using Buzz.Dto;
using Newtonsoft.Json;

namespace Buzz.Services;

public class WorkingDayCheckService
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly string _baseUrl;

    public WorkingDayCheckService(IConfiguration configuration)
    {
        _baseUrl = configuration["HolidayApiSettings:Url"];
    }

    public async Task<bool> IsWorkingDay(DateTime currentDate)
    {
        var year = currentDate.Year;
        var holidays = await GetHolidays(year);
        var holiday = holidays.Find(h => h.Date == currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        
        if (holiday != null) 
        {
            return !holiday.IsOffDay;
        }

        return !(currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday);
    }

    private async Task<List<HolidayDto>> GetHolidays(int year)
    {
        var url = $"{_baseUrl}/{year}.json";
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