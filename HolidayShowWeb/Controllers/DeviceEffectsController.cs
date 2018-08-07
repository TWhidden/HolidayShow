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
    public class DeviceEffectsController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public DeviceEffectsController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/DeviceEffects
        [HttpGet]
        public IEnumerable<DeviceEffects> GetDeviceEffects()
        {
            return _context.DeviceEffects;
        }

        // GET: api/DeviceEffects/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceEffects([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var deviceEffects = await _context.DeviceEffects.FindAsync(id);

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

            _context.Entry(deviceEffects).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> PostDeviceEffects([FromBody] DeviceEffects deviceEffects)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DeviceEffects.Add(deviceEffects);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDeviceEffects", new { id = deviceEffects.EffectId }, deviceEffects);
        }

        // DELETE: api/DeviceEffects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeviceEffects([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var deviceEffects = await _context.DeviceEffects.FindAsync(id);
            if (deviceEffects == null)
            {
                return NotFound();
            }

            _context.DeviceEffects.Remove(deviceEffects);
            await _context.SaveChangesAsync();

            return Ok(deviceEffects);
        }

        private bool DeviceEffectsExists(int id)
        {
            return _context.DeviceEffects.Any(e => e.EffectId == id);
        }
    }
}