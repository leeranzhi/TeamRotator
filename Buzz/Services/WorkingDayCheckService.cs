using System.Globalization;
using Buzz.Dto;
using Buzz.Model;
using Newtonsoft.Json;

namespace Buzz.Services;

public class WorkingDayCheckService(IConfiguration configuration, IHttpClientFactory httpClientFactory,
        ILogger<AssignmentUpdateService> logger)
    : IWorkingDayCheckService
{
    private readonly string _baseUrl = configuration["HolidayApiSettings:Url"];

    public async Task<bool> IsWorkingDayCheck(DateTime currentDate)
    {
        logger.LogInformation("Checking if {Date} is a working day.", currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        var year = currentDate.Year;
        var holidays = await GetHolidays(year);
        var holiday = holidays.Find(h => h.Date == currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        if (holiday != null)
        {
            logger.LogInformation("Found holiday on {Date}. IsOffDay: {IsOffDay}", currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), holiday.IsOffDay);
            return !holiday.IsOffDay;
        }

        bool isWorkingDay = !(currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday);
        logger.LogInformation("No holiday found. Checking weekend: {IsWorkingDay}", isWorkingDay);

        return isWorkingDay;
    }

    private async Task<List<HolidayDto>> GetHolidays(int year)
    {
        var url = $"{_baseUrl}/{year}.json";
        try
        {
            logger.LogInformation("Fetching holidays for year {Year} from {Url}", year, url);

            var client = httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();

            HolidaysResponse? holidays = JsonConvert.DeserializeObject<HolidaysResponse>(jsonContent);

            logger.LogInformation("Successfully fetched holidays for year {Year}", year);

            return holidays?.Days ?? new List<HolidayDto>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching JSON data for year {Year}", year);
            return new List<HolidayDto>();
        }
    }
}
