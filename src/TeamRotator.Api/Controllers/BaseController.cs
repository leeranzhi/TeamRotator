using Microsoft.AspNetCore.Mvc;

namespace TeamRotator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger<BaseController> Logger;

    protected BaseController(ILogger<BaseController> logger)
    {
        Logger = logger;
    }

    protected IActionResult HandleException(Exception ex)
    {
        Logger.LogError(ex, "An error occurred while processing the request");

        return ex switch
        {
            InvalidOperationException => BadRequest(ex.Message),
            ArgumentException => BadRequest(ex.Message),
            _ => StatusCode(500, "An unexpected error occurred")
        };
    }

    protected ActionResult<T> HandleException<T>(Exception ex)
    {
        Logger.LogError(ex, "An error occurred while processing the request");

        return ex switch
        {
            InvalidOperationException => BadRequest(ex.Message),
            ArgumentException => BadRequest(ex.Message),
            _ => StatusCode(500, "An unexpected error occurred")
        };
    }
} 