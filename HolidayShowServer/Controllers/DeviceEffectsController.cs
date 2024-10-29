using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeviceEffectsController(EfHolidayContext context) : ControllerBase
{
    // GET: api/DeviceEffects
    [HttpGet]
    public IEnumerable<DeviceEffects> GetDeviceEffects()
    {
        return context.DeviceEffects;
    }

    // GET: api/DeviceEffects/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeviceEffects([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var deviceEffects = await context.DeviceEffects.FindAsync(id);

        if (deviceEffects == null)
        {
            return NotFound();
        }

        return Ok(deviceEffects);
    }

    // PUT: api/DeviceEffects/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDeviceEffects([FromRoute] int id, [FromBody] DeviceEffects deviceEffects)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != deviceEffects.EffectId)
        {
            return BadRequest();
        }

        context.Entry(deviceEffects).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DeviceEffectsExists(id))
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

    // POST: api/DeviceEffects
    [HttpPost]
    public async Task<ActionResult<DeviceEffects>> PostDeviceEffects([FromBody] DeviceEffects deviceEffects)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.DeviceEffects.Add(deviceEffects);
        await context.SaveChangesAsync();

        return Ok(deviceEffects);
    }

    // DELETE: api/DeviceEffects/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeviceEffects([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var deviceEffects = await context.DeviceEffects.FindAsync(id);
        if (deviceEffects == null)
        {
            return NotFound();
        }

        context.DeviceEffects.Remove(deviceEffects);
        await context.SaveChangesAsync();

        return Ok(deviceEffects);
    }

    private bool DeviceEffectsExists(int id)
    {
        return context.DeviceEffects.Any(e => e.EffectId == id);
    }
}