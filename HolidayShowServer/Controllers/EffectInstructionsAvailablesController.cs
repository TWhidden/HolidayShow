using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EffectInstructionsAvailablesController(EfHolidayContext context) : ControllerBase
{
    // GET: api/EffectInstructionsAvailables
    [HttpGet]
    public IEnumerable<EffectInstructionsAvailable> GetEffectInstructionsAvailable()
    {
        return context.EffectInstructionsAvailable;
    }

    // GET: api/EffectInstructionsAvailables/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEffectInstructionsAvailable([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var effectInstructionsAvailable = await context.EffectInstructionsAvailable.FindAsync(id);

        if (effectInstructionsAvailable == null)
        {
            return NotFound();
        }

        return Ok(effectInstructionsAvailable);
    }

    // PUT: api/EffectInstructionsAvailables/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEffectInstructionsAvailable([FromRoute] int id, [FromBody] EffectInstructionsAvailable effectInstructionsAvailable)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != effectInstructionsAvailable.EffectInstructionId)
        {
            return BadRequest();
        }

        context.Entry(effectInstructionsAvailable).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EffectInstructionsAvailableExists(id))
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

    // POST: api/EffectInstructionsAvailables
    [HttpPost]
    public async Task<IActionResult> PostEffectInstructionsAvailable([FromBody] EffectInstructionsAvailable effectInstructionsAvailable)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.EffectInstructionsAvailable.Add(effectInstructionsAvailable);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetEffectInstructionsAvailable", new { id = effectInstructionsAvailable.EffectInstructionId }, effectInstructionsAvailable);
    }

    // DELETE: api/EffectInstructionsAvailables/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEffectInstructionsAvailable([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var effectInstructionsAvailable = await context.EffectInstructionsAvailable.FindAsync(id);
        if (effectInstructionsAvailable == null)
        {
            return NotFound();
        }

        context.EffectInstructionsAvailable.Remove(effectInstructionsAvailable);
        await context.SaveChangesAsync();

        return Ok(effectInstructionsAvailable);
    }

    private bool EffectInstructionsAvailableExists(int id)
    {
        return context.EffectInstructionsAvailable.Any(e => e.EffectInstructionId == id);
    }
}