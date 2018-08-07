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
    public class SettingsController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public SettingsController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/Settings
        [HttpGet]
        public IEnumerable<Settings> GetSettings()
        {
            return _context.Settings;
        }

        // GET: api/Settings/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSettings([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settings = await _context.Settings.FindAsync(id);

            if (settings == null)
            {
                return NotFound();
            }

            return Ok(settings);
        }

        // PUT: api/Settings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSettings([FromRoute] string id, [FromBody] Settings settings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != settings.SettingName)
            {
                return BadRequest();
            }

            _context.Entry(settings).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SettingsExists(id))
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

        // POST: api/Settings
        [HttpPost]
        public async Task<IActionResult> PostSettings([FromBody] Settings settings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Settings.Add(settings);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SettingsExists(settings.SettingName))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSettings", new { id = settings.SettingName }, settings);
        }

        // DELETE: api/Settings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSettings([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settings = await _context.Settings.FindAsync(id);
            if (settings == null)
            {
                return NotFound();
            }

            _context.Settings.Remove(settings);
            await _context.SaveChangesAsync();

            return Ok(settings);
        }

        private bool SettingsExists(string id)
        {
            return _context.Settings.Any(e => e.SettingName == id);
        }
    }
}