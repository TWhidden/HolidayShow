using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HolidayShow.Data;

namespace HolidayShowWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicePatternSequencesController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public DevicePatternSequencesController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/DevicePatternSequences
        [HttpGet]
        public IEnumerable<DevicePatternSequences> GetDevicePatternSequences()
        {
            return _context.DevicePatternSequences;
        }

        [HttpGet("SequenceByPatternId/{id}")]
        public IEnumerable<DevicePatternSequences> GetDevicePatternSequencesByPatternId([FromRoute] int id)
        {
            return _context.DevicePatternSequences.Where(x => x.DevicePatternId == id);
        }

        // GET: api/DevicePatternSequences/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevicePatternSequences([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var devicePatternSequences = await _context.DevicePatternSequences.FindAsync(id);

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

            _context.Entry(devicePatternSequences).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> PostDevicePatternSequences([FromRoute] int deviceId, [FromBody] DevicePatternSequences devicePatternSequences)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (devicePatternSequences.AudioId == 0)
            {
                devicePatternSequences.AudioId = await _context.AudioOptions.Where(x => x.Name == "NONE")
                    .Select(x => x.AudioId).FirstOrDefaultAsync();
            }

            if (devicePatternSequences.DeviceIoPortId == 0)
            {
                // ensure there is a -1 pin id at the very least.
                var pin = await _context.DeviceIoPorts.Where(x => x.DeviceId == deviceId && x.CommandPin == -1).FirstOrDefaultAsync();

                if (pin == null)
                {
                    pin = new DeviceIoPorts()
                    {
                        DeviceId = deviceId,
                        CommandPin = -1,
                        Description = "NONE",
                    };
                    _context.DeviceIoPorts.Add(pin);
                    await _context.SaveChangesAsync();
                }

                devicePatternSequences.DeviceIoPortId = pin.DeviceIoPortId;
            }

            _context.DevicePatternSequences.Add(devicePatternSequences);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDevicePatternSequences", new { id = devicePatternSequences.DevicePatternSeqenceId }, devicePatternSequences);
        }

        // DELETE: api/DevicePatternSequences/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevicePatternSequences([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var devicePatternSequences = await _context.DevicePatternSequences.FindAsync(id);
            if (devicePatternSequences == null)
            {
                return NotFound();
            }

            _context.DevicePatternSequences.Remove(devicePatternSequences);
            await _context.SaveChangesAsync();

            return Ok(devicePatternSequences);
        }

        private bool DevicePatternSequencesExists(int id)
        {
            return _context.DevicePatternSequences.Any(e => e.DevicePatternSeqenceId == id);
        }
    }
}