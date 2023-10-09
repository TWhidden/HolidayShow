using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HolidayShow.Data;
using HolidayShow.Data.Core;

namespace HolidayShowWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceIoPortsController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public DeviceIoPortsController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/DeviceIoPorts
        [HttpGet]
        public IEnumerable<DeviceIoPorts> GetDeviceIoPorts()
        {
            return _context.DeviceIoPorts;
        }

        // GET: api/DeviceIoPorts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceIoPorts([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var deviceIoPorts = await _context.DeviceIoPorts.FindAsync(id);

            if (deviceIoPorts == null)
            {
                return NotFound();
            }

            return Ok(deviceIoPorts);
        }

        [HttpGet("ByDeviceId/{deviceId}")]
        public async Task<IActionResult> GetDeviceIoPortsByDeviceId([FromRoute] int deviceId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var deviceIoPorts = await _context.DeviceIoPorts.Where(x => x.DeviceId == deviceId).OrderBy(x => x.CommandPin).ToListAsync();

            if (deviceIoPorts == null)
            {
                return NotFound();
            }

            return Ok(deviceIoPorts);
        }

        // PUT: api/DeviceIoPorts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeviceIoPorts([FromRoute] int id, [FromBody] DeviceIoPorts deviceIoPorts)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != deviceIoPorts.DeviceIoPortId)
            {
                return BadRequest();
            }

            _context.Entry(deviceIoPorts).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceIoPortsExists(id))
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

        // PUT: api/DeviceIoPorts/PutDeviceIoPortIdentify/5
        [HttpPut("PutDeviceIoPortIdentify/{id}")]
        public async Task<IActionResult> PutDeviceIoPortIdentify([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                
                    var ioPort = await _context.DeviceIoPorts.Where(x => x.DeviceIoPortId == id).FirstOrDefaultAsync();
                    if (ioPort == null) return BadRequest(ModelState);

                    var existingSetting = await _context.Settings.Where(x => x.SettingName == SettingKeys.DetectDevicePin)
                        .FirstOrDefaultAsync();
                    if (existingSetting == null)
                    {
                        existingSetting = new Settings()
                        {
                            SettingName = SettingKeys.DetectDevicePin
                        };
                        _context.Settings.Add(existingSetting);
                    }

                    existingSetting.ValueString = $"{ioPort.DeviceId}:{ioPort.CommandPin}";
                    await _context.SaveChangesAsync();

                    var option = await _context.Settings.Where(x => x.SettingName == SettingKeys.SetPlaybackOption)
                        .FirstOrDefaultAsync();
                    if (option == null)
                    {
                        option = new Settings {SettingName = SettingKeys.SetPlaybackOption};
                        _context.Settings.Add(option);
                    }

                    option.ValueDouble = (double) SetPlaybackOptionEnum.DevicePinDetect;
                    await _context.SaveChangesAsync();

                    var setting = await _context.Settings.Where(x => x.SettingName == SettingKeys.Refresh).FirstOrDefaultAsync();

                    if (setting == null)
                    {
                        setting = new Settings
                        {
                            SettingName = SettingKeys.Refresh,
                        };
                        _context.Settings.Add(setting);
                    }

                    setting.ValueString = "None";

                    await _context.SaveChangesAsync();
                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceIoPortsExists(id))
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

        // POST: api/DeviceIoPorts
        [HttpPost]
        public async Task<IActionResult> PostDeviceIoPorts([FromBody] DeviceIoPorts deviceIoPorts)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DeviceIoPorts.Add(deviceIoPorts);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDeviceIoPorts", new { id = deviceIoPorts.DeviceIoPortId }, deviceIoPorts);
        }

        // DELETE: api/DeviceIoPorts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeviceIoPorts([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var deviceIoPorts = await _context.DeviceIoPorts.FindAsync(id);
            if (deviceIoPorts == null)
            {
                return NotFound();
            }

            _context.DeviceIoPorts.Remove(deviceIoPorts);
            await _context.SaveChangesAsync();

            return Ok(deviceIoPorts);
        }

        private bool DeviceIoPortsExists(int id)
        {
            return _context.DeviceIoPorts.Any(e => e.DeviceIoPortId == id);
        }
    }
}