using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamRotator.Core.Entities;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Api.Controllers;

[ApiController]
public class MembersController : BaseController
{
    private readonly IDbContextFactory<RotationDbContext> _contextFactory;

    public MembersController(
        ILogger<MembersController> logger,
        IDbContextFactory<RotationDbContext> contextFactory)
        : base(logger)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<Member>>> GetMembers()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var members = await context.Members.ToListAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            return HandleException<List<Member>>(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var member = await context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }
        catch (Exception ex)
        {
            return HandleException<Member>(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember([FromBody] Member member)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Members.Add(member);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
        }
        catch (Exception ex)
        {
            return HandleException<Member>(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Member member)
    {
        if (id != member.Id)
        {
            return BadRequest();
        }

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Entry(member).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!await context.Members.AnyAsync(m => m.Id == id))
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
    public async Task<IActionResult> DeleteMember(int id)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var member = await context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            context.Members.Remove(member);
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
} 