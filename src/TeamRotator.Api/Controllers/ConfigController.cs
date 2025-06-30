using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace TeamRotator.Api.Controllers;

[ApiController]
public class ConfigController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly string _configPath;

    public ConfigController(
        ILogger<ConfigController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
        : base(logger)
    {
        _configuration = configuration;
        _environment = environment;
        _configPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
    }

    [HttpGet("webhook-url")]
    public ActionResult<string> GetWebhookUrl()
    {
        try
        {
            var webhookUrl = _configuration["Slack:WebhookUrl"];
            return Ok(webhookUrl);
        }
        catch (Exception ex)
        {
            return HandleException<string>(ex);
        }
    }

    [HttpPost("webhook-url")]
    public async Task<ActionResult> UpdateWebhookUrl([FromBody] string webhookUrl)
    {
        try
        {
            var jsonString = await File.ReadAllTextAsync(_configPath);
            var config = JsonSerializer.Deserialize<JsonDocument>(jsonString);
            var root = config.RootElement.Clone();
            var configObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(root.GetRawText());

            if (!configObject.ContainsKey("Slack"))
            {
                configObject["Slack"] = JsonSerializer.SerializeToElement(new { WebhookUrl = webhookUrl });
            }
            else
            {
                var slack = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configObject["Slack"].GetRawText());
                slack["WebhookUrl"] = JsonSerializer.SerializeToElement(webhookUrl);
                configObject["Slack"] = JsonSerializer.SerializeToElement(slack);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(configObject, options);
            await File.WriteAllTextAsync(_configPath, updatedJson);

            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
} 