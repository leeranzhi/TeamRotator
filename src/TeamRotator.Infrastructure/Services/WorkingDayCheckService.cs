using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeamRotator.Core.DTOs;
using TeamRotator.Core.Interfaces;

namespace TeamRotator.Infrastructure.Services;

public class WorkingDayCheckService : IWorkingDayCheckService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WorkingDayCheckService> _logger;
    private readonly string _baseUrl;

    public WorkingDayCheckService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<WorkingDayCheckService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _baseUrl = configuration["HolidayApiSettings:Url"] ?? throw new InvalidOperationException("Holiday API URL not configured");
    }

    public async Task<bool> IsWorkingDayCheck(DateTime currentDate)
    {
        _logger.LogInformation("Checking if {Date} is a working day.", currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        var year = currentDate.Year;
        var holidays = await GetHolidays(year);
        var holiday = holidays.Find(h => h.Date == currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        if (holiday != null)
        {
            _logger.LogInformation("Found holiday on {Date}. IsOffDay: {IsOffDay}", 
                currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), holiday.IsOffDay);
            return !holiday.IsOffDay;
        }

        bool isWorkingDay = !(currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday);
        _logger.LogInformation("No holiday found. Checking weekend: {IsWorkingDay}", isWorkingDay);

        return isWorkingDay;
    }

    private async Task<List<HolidayDto>> GetHolidays(int year)
    {
        var url = $"{_baseUrl}/{year}.json";
        try
        {
            _logger.LogInformation("Fetching holidays for year {Year} from {Url}", year, url);

            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();

            var holidays = JsonSerializer.Deserialize<HolidaysResponseDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully fetched holidays for year {Year}", year);

            return holidays?.Days ?? new List<HolidayDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching JSON data for year {Year}", year);
            return new List<HolidayDto>();
        }
    }
} 