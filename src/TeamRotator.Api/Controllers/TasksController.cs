using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.Entities;
using TeamRotator.Infrastructure.Data;
using Task = TeamRotator.Core.Entities.Task;

namespace TeamRotator.Api.Controllers;

[ApiController]
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
    public async Task<ActionResult<List<Task>>> GetTasks()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tasks = await context.Tasks.ToListAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return HandleException<List<Task>>(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Task>> GetTask(int id)
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
            return HandleException<Task>(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Task>> CreateTask([FromBody] Task task)
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
            return HandleException<Task>(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] Task task)
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