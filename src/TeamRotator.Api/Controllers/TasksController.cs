using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.Entities;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Api.Controllers;

public class TasksController : BaseController
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;

    public TasksController(
        ILogger<TasksController> logger,
        IDbContextFactory<RotationDbContext> contextFactory)
        : base(logger)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<RotationTask>>> GetTasks()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tasks = await context.Tasks.ToListAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return HandleException<List<RotationTask>>(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RotationTask>> GetTask(int id)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var task = await context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }
        catch (Exception ex)
        {
            return HandleException<RotationTask>(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<RotationTask>> CreateTask([FromBody] RotationTask task)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            return HandleException<RotationTask>(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] RotationTask task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Entry(task).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!await context.Tasks.AnyAsync(t => t.Id == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var task = await context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
} 