using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HolidayShow.Data;
using HolidayShow.Data.Core;
using DevicePatterns = HolidayShow.Data.Core.DevicePatterns;

namespace HolidayShowWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicePatternsController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public DevicePatternsController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/DevicePatterns
        [HttpGet]
        public IEnumerable<DevicePatterns> GetDevicePatterns()
        {
            return _context.DevicePatterns;
        }

        // GET: api/DevicePatterns/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevicePatterns([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var devicePatterns = await _context.DevicePatterns.FindAsync(id);

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

            var devicePatterns = await _context.DevicePatterns.Where(x => x.DeviceId == id).ToArrayAsync();

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

            _context.Entry(devicePatterns).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> PostDevicePatterns([FromBody] DevicePatterns devicePatterns)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DevicePatterns.Add(devicePatterns);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDevicePatterns", new { id = devicePatterns.DevicePatternId }, devicePatterns);
        }

        // DELETE: api/DevicePatterns/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevicePatterns([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var devicePatterns = await _context.DevicePatterns.FindAsync(id);
            if (devicePatterns == null)
            {
                return NotFound();
            }

            _context.DevicePatterns.Remove(devicePatterns);
            await _context.SaveChangesAsync();

            return Ok(devicePatterns);
        }

        private bool DevicePatternsExists(int id)
        {
            return _context.DevicePatterns.Any(e => e.DevicePatternId == id);
        }
    }
}