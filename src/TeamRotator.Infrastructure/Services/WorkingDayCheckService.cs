using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeamRotator.Core.Interfaces;

namespace TeamRotator.Infrastructure.Services;

public class WorkingDayCheckService : IWorkingDayCheckService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkingDayCheckService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _holidayApiUrl;

    public WorkingDayCheckService(
        IConfiguration configuration,
        ILogger<WorkingDayCheckService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _holidayApiUrl = configuration["HolidayApiSettings:Url"] ?? throw new InvalidOperationException("Holiday API URL not configured");
    }

    public async Task<bool> IsWorkingDayCheck(DateTime date)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{_holidayApiUrl}/holidays.json");
            var holidays = JsonSerializer.Deserialize<Dictionary<string, bool>>(response)
                ?? throw new InvalidOperationException("Failed to deserialize holiday data");

            var dateString = date.ToString("yyyy-MM-dd");
            return !holidays.ContainsKey(dateString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if {Date} is a working day", date);
            throw;
        }
    }
} 