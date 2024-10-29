using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DevicesController(EfHolidayContext context) : ControllerBase
{
    // GET: api/Devices
    [HttpGet]
    public IEnumerable<Devices> GetDevices()
    {
        return context.Devices;
    }

    // GET: api/Devices/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDevices([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devices = await context.Devices.FindAsync(id);

        if (devices == null)
        {
            return NotFound();
        }

        return Ok(devices);
    }

    // PUT: api/Devices/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDevices([FromRoute] int id, [FromBody] Devices devices)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != devices.DeviceId)
        {
            return BadRequest();
        }

        context.Entry(devices).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DevicesExists(id))
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

    // POST: api/Devices
    [HttpPost]
    public async Task<IActionResult> PostDevices([FromBody] Devices devices)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.Devices.Add(devices);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (DevicesExists(devices.DeviceId))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            else
            {
                throw;
            }
        }

        return CreatedAtAction("GetDevices", new { id = devices.DeviceId }, devices);
    }

    // DELETE: api/Devices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevices([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devices = await context.Devices.FindAsync(id);
        if (devices == null)
        {
            return NotFound();
        }

        context.Devices.Remove(devices);
        await context.SaveChangesAsync();

        return Ok(devices);
    }

    private bool DevicesExists(int id)
    {
        return context.Devices.Any(e => e.DeviceId == id);
    }
}