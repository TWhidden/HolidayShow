using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeviceIoPortsController(EfHolidayContext context) : ControllerBase
{
    // GET: api/DeviceIoPorts
    [HttpGet]
    public IEnumerable<DeviceIoPorts> GetDeviceIoPorts()
    {
        return context.DeviceIoPorts;
    }

    // GET: api/DeviceIoPorts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceIoPorts>> GetDeviceIoPorts([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var deviceIoPorts = await context.DeviceIoPorts.FindAsync(id);

        if (deviceIoPorts == null)
        {
            return NotFound();
        }

        return Ok(deviceIoPorts);
    }

    [HttpGet("ByDeviceId/{deviceId}")]
    public async Task<ActionResult<DeviceIoPorts[]>> GetDeviceIoPortsByDeviceId([FromRoute] int deviceId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var deviceIoPorts = await context.DeviceIoPorts.Where(x => x.DeviceId == deviceId).OrderBy(x => x.CommandPin).ToArrayAsync();

        return Ok(deviceIoPorts);
    }

    // PUT: api/DeviceIoPorts/5
    [HttpPut("{id}")]
    public async Task<ActionResult> PutDeviceIoPorts([FromRoute] int id, [FromBody] DeviceIoPorts deviceIoPorts)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != deviceIoPorts.DeviceIoPortId)
        {
            return BadRequest();
        }

        context.Entry(deviceIoPorts).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
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
    public async Task<ActionResult> PutDeviceIoPortIdentify([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
                
            var ioPort = await context.DeviceIoPorts.Where(x => x.DeviceIoPortId == id).FirstOrDefaultAsync();
            if (ioPort == null) return BadRequest(ModelState);

            var existingSetting = await context.Settings.Where(x => x.SettingName == SettingKeys.DetectDevicePin)
                .FirstOrDefaultAsync();
            if (existingSetting == null)
            {
                existingSetting = new Settings()
                {
                    SettingName = SettingKeys.DetectDevicePin
                };
                context.Settings.Add(existingSetting);
            }

            existingSetting.ValueString = $"{ioPort.DeviceId}:{ioPort.CommandPin}";
            await context.SaveChangesAsync();

            var option = await context.Settings.Where(x => x.SettingName == SettingKeys.SetPlaybackOption)
                .FirstOrDefaultAsync();
            if (option == null)
            {
                option = new Settings {SettingName = SettingKeys.SetPlaybackOption};
                context.Settings.Add(option);
            }

            option.ValueDouble = (double) SetPlaybackOptionEnum.DevicePinDetect;
            await context.SaveChangesAsync();

            var setting = await context.Settings.Where(x => x.SettingName == SettingKeys.Refresh).FirstOrDefaultAsync();

            if (setting == null)
            {
                setting = new Settings
                {
                    SettingName = SettingKeys.Refresh,
                };
                context.Settings.Add(setting);
            }

            setting.ValueString = "None";

            await context.SaveChangesAsync();
                
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
    public async Task<ActionResult<DeviceIoPorts>> PostDeviceIoPorts([FromBody] DeviceIoPorts deviceIoPorts)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        context.DeviceIoPorts.Add(deviceIoPorts);
        await context.SaveChangesAsync();

        return Ok(deviceIoPorts);
    }

    // DELETE: api/DeviceIoPorts/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDeviceIoPorts([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var deviceIoPorts = await context.DeviceIoPorts.FindAsync(id);
        if (deviceIoPorts == null)
        {
            return NotFound();
        }

        context.DeviceIoPorts.Remove(deviceIoPorts);
        await context.SaveChangesAsync();

        return Ok();
    }

    private bool DeviceIoPortsExists(int id)
    {
        return context.DeviceIoPorts.Any(e => e.DeviceIoPortId == id);
    }
}