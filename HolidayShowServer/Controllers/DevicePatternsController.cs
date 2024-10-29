using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevicePatterns = HolidayShow.Data.Core.DevicePatterns;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DevicePatternsController(EfHolidayContext context) : ControllerBase
{
    // GET: api/DevicePatterns
    [HttpGet]
    public IEnumerable<DevicePatterns> GetDevicePatterns()
    {
        return context.DevicePatterns;
    }

    // GET: api/DevicePatterns/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDevicePatterns([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devicePatterns = await context.DevicePatterns.FindAsync(id);

        if (devicePatterns == null)
        {
            return NotFound();
        }

        return Ok(devicePatterns);
    }

    // GET: api/DevicePatterns/GetDevicePatternsByDevice/5
    [HttpGet("GetDevicePatternsByDeviceId/{id}")]
    public async Task<IActionResult> GetDevicePatternsByDeviceId([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devicePatterns = await context.DevicePatterns.Where(x => x.DeviceId == id).ToArrayAsync();

        if (devicePatterns == null)
        {
            return NotFound();
        }

        return Ok(devicePatterns);
    }

    // PUT: api/DevicePatterns/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDevicePatterns([FromRoute] int id, [FromBody] DevicePatterns devicePatterns)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != devicePatterns.DevicePatternId)
        {
            return BadRequest();
        }

        context.Entry(devicePatterns).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DevicePatternsExists(id))
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

    // POST: api/DevicePatterns
    [HttpPost]
    [ProducesResponseType<DevicePatterns>(200)]
    public async Task<ActionResult<DevicePatterns>> PostDevicePatterns([FromBody] DevicePatterns devicePatterns)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.DevicePatterns.Add(devicePatterns);
        await context.SaveChangesAsync();

        return Ok(devicePatterns);
    }

    // DELETE: api/DevicePatterns/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevicePatterns([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devicePatterns = await context.DevicePatterns.FindAsync(id);
        if (devicePatterns == null)
        {
            return NotFound();
        }

        context.DevicePatterns.Remove(devicePatterns);
        await context.SaveChangesAsync();

        return Ok(devicePatterns);
    }

    private bool DevicePatternsExists(int id)
    {
        return context.DevicePatterns.Any(e => e.DevicePatternId == id);
    }
}