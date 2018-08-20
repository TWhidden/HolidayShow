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
    public class DevicesController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public DevicesController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet]
        public IEnumerable<Devices> GetDevices()
        {
            return _context.Devices;
        }

        // GET: api/Devices/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevices([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var devices = await _context.Devices.FindAsync(id);

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

            _context.Entry(devices).State = EntityState.Modified;

            foreach (var devicesDeviceIoPort in devices.DeviceIoPorts)
            {
                _context.Entry(devicesDeviceIoPort).State = EntityState.Modified;
            }

            await Task.Delay(5000);

            try
            {
                await _context.SaveChangesAsync();
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

            _context.Devices.Add(devices);
            try
            {
                await _context.SaveChangesAsync();
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

            var devices = await _context.Devices.FindAsync(id);
            if (devices == null)
            {
                return NotFound();
            }

            _context.Devices.Remove(devices);
            await _context.SaveChangesAsync();

            return Ok(devices);
        }

        private bool DevicesExists(int id)
        {
            return _context.Devices.Any(e => e.DeviceId == id);
        }
    }
}