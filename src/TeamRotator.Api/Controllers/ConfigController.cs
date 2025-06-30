using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.Entities;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Api.Controllers;

[ApiController]
public class ConfigController : BaseController
{
    private readonly RotationDbContext _dbContext;
    private const string WEBHOOK_URL_KEY = "Slack:WebhookUrl";

    public ConfigController(
        ILogger<ConfigController> logger,
        RotationDbContext dbContext)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    [HttpGet("webhook-url")]
    public async Task<ActionResult<string>> GetWebhookUrl()
    {
        try
        {
            var config = await _dbContext.SystemConfigs
                .FirstOrDefaultAsync(c => c.Key == WEBHOOK_URL_KEY);
            return Ok(config?.Value ?? string.Empty);
        }
        catch (Exception ex)
        {
            return HandleException<string>(ex);
        }
    }

    [HttpPost("webhook-url")]
    public async Task<IActionResult> UpdateWebhookUrl([FromBody] string webhookUrl)
    {
        try
        {
            var config = await _dbContext.SystemConfigs
                .FirstOrDefaultAsync(c => c.Key == WEBHOOK_URL_KEY);

            if (config == null)
            {
                config = new SystemConfig
                {
                    Key = WEBHOOK_URL_KEY,
                    Value = webhookUrl,
                    LastModified = DateTime.UtcNow
                };
                _dbContext.SystemConfigs.Add(config);
            }
            else
            {
                config.Value = webhookUrl;
                config.LastModified = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
} 