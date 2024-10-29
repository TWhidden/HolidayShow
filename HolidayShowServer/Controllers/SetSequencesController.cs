using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SetSequencesController(EfHolidayContext context) : ControllerBase
{
    // GET: api/SetSequences
    [HttpGet]
    public IEnumerable<SetSequences> GetSetSequences()
    {
        return context.SetSequences;
    }

    // GET: api/SetSequences/5
    [HttpGet("SetSequencesBySetId/{setId}")]
    public IEnumerable<SetSequences> GetSetSequencesBySetId([FromRoute] int setId)
    {
        return context.SetSequences.Where(x => x.SetId == setId);
    }

    // GET: api/SetSequences/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSetSequences([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var setSequences = await context.SetSequences.FindAsync(id);

        if (setSequences == null)
        {
            return NotFound();
        }

        return Ok(setSequences);
    }

    // PUT: api/SetSequences/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSetSequences([FromRoute] int id, [FromBody] SetSequences setSequences)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != setSequences.SetSequenceId)
        {
            return BadRequest();
        }

        context.Entry(setSequences).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SetSequencesExists(id))
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

    // POST: api/SetSequences
    [HttpPost]
    public async Task<IActionResult> PostSetSequences([FromBody] SetSequences setSequences)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.SetSequences.Add(setSequences);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetSetSequences", new { id = setSequences.SetSequenceId }, setSequences);
    }

    // DELETE: api/SetSequences/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSetSequences([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var setSequences = await context.SetSequences.FindAsync(id);
        if (setSequences == null)
        {
            return NotFound();
        }

        context.SetSequences.Remove(setSequences);
        await context.SaveChangesAsync();

        return Ok(setSequences);
    }

    private bool SetSequencesExists(int id)
    {
        return context.SetSequences.Any(e => e.SetSequenceId == id);
    }
}