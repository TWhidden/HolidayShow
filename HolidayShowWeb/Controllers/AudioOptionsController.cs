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
    public class AudioOptionsController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public AudioOptionsController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/AudioOptions
        [HttpGet]
        public IEnumerable<AudioOptions> GetAudioOptions()
        {
            return _context.AudioOptions;
        }

        // GET: api/AudioOptions/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudioOptions([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var audioOptions = await _context.AudioOptions.FindAsync(id);

            if (audioOptions == null)
            {
                return NotFound();
            }

            return Ok(audioOptions);
        }

        // PUT: api/AudioOptions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAudioOptions([FromRoute] int id, [FromBody] AudioOptions audioOptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != audioOptions.AudioId)
            {
                return BadRequest();
            }

            _context.Entry(audioOptions).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AudioOptionsExists(id))
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

        // POST: api/AudioOptions
        [HttpPost]
        public async Task<IActionResult> PostAudioOptions([FromBody] AudioOptions audioOptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.AudioOptions.Add(audioOptions);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAudioOptions", new { id = audioOptions.AudioId }, audioOptions);
        }

        // DELETE: api/AudioOptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAudioOptions([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var audioOptions = await _context.AudioOptions.FindAsync(id);
            if (audioOptions == null)
            {
                return NotFound();
            }

            _context.AudioOptions.Remove(audioOptions);
            await _context.SaveChangesAsync();

            return Ok(audioOptions);
        }

        private bool AudioOptionsExists(int id)
        {
            return _context.AudioOptions.Any(e => e.AudioId == id);
        }
    }
}