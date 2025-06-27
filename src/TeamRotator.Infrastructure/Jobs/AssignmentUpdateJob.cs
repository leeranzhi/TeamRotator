using Microsoft.Extensions.Logging;
using Quartz;
using Serilog.Context;
using TeamRotator.Core.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TeamRotator.Infrastructure.Jobs;

public class AssignmentUpdateJob : IJob
{
    private readonly IRotationService _rotationService;
    private readonly ILogger<AssignmentUpdateJob> _logger;

    public AssignmentUpdateJob(IRotationService rotationService, ILogger<AssignmentUpdateJob> logger)
    {
        _rotationService = rotationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var correlationIdScope = LogContext.PushProperty("CorrelationId", Guid.NewGuid());
        _logger.LogInformation("Executing quartz AssignmentUpdateJob...");

        try
        {
            await Task.Run(() => _rotationService.UpdateTaskAssignmentList());
             
            _logger.LogInformation("Successfully auto updated task assignments.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while auto updating task assignments.");
            throw;
        }
    }
} 