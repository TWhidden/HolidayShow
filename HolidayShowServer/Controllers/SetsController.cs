using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SetsController(EfHolidayContext context) : ControllerBase
{
    // GET: api/Sets
    [HttpGet]
    public IEnumerable<Sets> GetSets()
    {
        return context.Sets;
    }

    // GET: api/Sets/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ICollection<Sets>>> GetSets([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sets = await context.Sets.FindAsync(id);

        if (sets == null)
        {
            return NotFound();
        }

        return Ok(sets);
    }

    // PUT: api/Sets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSets([FromRoute] int id, [FromBody] Sets sets)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != sets.SetId)
        {
            return BadRequest();
        }

        context.Entry(sets).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SetsExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Sets
    [HttpPost]
    public async Task<ActionResult<Sets>> PostSets([FromBody] Sets sets)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.Sets.Add(sets);
        await context.SaveChangesAsync();

        return Ok(sets);
    }

    // DELETE: api/Sets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSets([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sets = await context.Sets.FindAsync(id);
        if (sets == null)
        {
            return NotFound();
        }

        context.Sets.Remove(sets);
        await context.SaveChangesAsync();

        return Ok(sets);
    }

    private bool SetsExists(int id)
    {
        return context.Sets.Any(e => e.SetId == id);
    }
}