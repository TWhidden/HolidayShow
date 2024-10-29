using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DevicePatternSequencesController(EfHolidayContext context) : ControllerBase
{
    // GET: api/DevicePatternSequences
    [HttpGet]
    public IEnumerable<DevicePatternSequences> GetDevicePatternSequences()
    {
        return context.DevicePatternSequences;
    }

    [HttpGet("SequenceByPatternId/{id}")]
    public IEnumerable<DevicePatternSequences> GetDevicePatternSequencesByPatternId([FromRoute] int id)
    {
        return context.DevicePatternSequences.Where(x => x.DevicePatternId == id).OrderBy(x => x.OnAt);
    }

    // GET: api/DevicePatternSequences/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DevicePatternSequences>> GetDevicePatternSequences([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devicePatternSequences = await context.DevicePatternSequences.FindAsync(id);

        if (devicePatternSequences == null)
        {
            return NotFound();
        }

        return Ok(devicePatternSequences);
    }

    // PUT: api/DevicePatternSequences/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDevicePatternSequences([FromRoute] int id, [FromBody] DevicePatternSequences devicePatternSequences)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != devicePatternSequences.DevicePatternSeqenceId)
        {
            return BadRequest();
        }

        context.Entry(devicePatternSequences).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DevicePatternSequencesExists(id))
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

    // POST: api/DevicePatternSequences/5
    [HttpPost("{deviceId}")]
    public async Task<ActionResult<DevicePatternSequences>> PostDevicePatternSequences([FromRoute] int deviceId, [FromBody] DevicePatternSequences devicePatternSequences)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (devicePatternSequences.AudioId == 0)
        {
            devicePatternSequences.AudioId = await context.AudioOptions.Where(x => x.Name == "NONE")
                .Select(x => x.AudioId).FirstOrDefaultAsync();
        }

        if (devicePatternSequences.DeviceIoPortId == 0)
        {
            // ensure there is a -1 pin id at the very least.
            var pin = await context.DeviceIoPorts.Where(x => x.DeviceId == deviceId && x.CommandPin == -1).FirstOrDefaultAsync();

            if (pin == null)
            {
                pin = new DeviceIoPorts()
                {
                    DeviceId = deviceId,
                    CommandPin = -1,
                    Description = "NONE",
                };
                context.DeviceIoPorts.Add(pin);
                await context.SaveChangesAsync();
            }

            devicePatternSequences.DeviceIoPortId = pin.DeviceIoPortId;
        }

        context.DevicePatternSequences.Add(devicePatternSequences);
        await context.SaveChangesAsync();

        return Ok(devicePatternSequences);
    }

    // DELETE: api/DevicePatternSequences/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevicePatternSequences([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var devicePatternSequences = await context.DevicePatternSequences.FindAsync(id);
        if (devicePatternSequences == null)
        {
            return NotFound();
        }

        context.DevicePatternSequences.Remove(devicePatternSequences);
        await context.SaveChangesAsync();

        return Ok(devicePatternSequences);
    }

    private bool DevicePatternSequencesExists(int id)
    {
        return context.DevicePatternSequences.Any(e => e.DevicePatternSeqenceId == id);
    }
}