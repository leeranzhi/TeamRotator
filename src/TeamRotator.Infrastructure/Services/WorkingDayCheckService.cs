using System.Text.Json;
using TeamRotator.Core.Interfaces;

namespace TeamRotator.Infrastructure.Services;

public class WorkingDayCheckService : IWorkingDayCheckService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkingDayCheckService> _logger;

    public WorkingDayCheckService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WorkingDayCheckService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsWorkingDayCheck(DateTime date)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["HolidayApiSettings:Url"];
            var year = date.Year;
            var url = $"{baseUrl}/{year}.json";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get holiday data from API. Status code: {StatusCode}", response.StatusCode);
                return true;
            }

            var content = await response.Content.ReadAsStringAsync();
            var holidays = JsonSerializer.Deserialize<Dictionary<string, int>>(content);

            if (holidays == null)
            {
                _logger.LogWarning("Failed to deserialize holiday data");
                return true;
            }

            var dateString = date.ToString("yyyy-MM-dd");
            return !holidays.ContainsKey(dateString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking working day status");
            return true;
        }
    }
} 