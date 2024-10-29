using HolidayShow.Data.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AudioOptions = HolidayShow.Data.Core.AudioOptions;

namespace HolidayShowServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AudioOptionsController(EfHolidayContext context) : ControllerBase
{
    // GET: api/AudioOptions
    [HttpGet]
    public IEnumerable<AudioOptions> GetAudioOptions()
    {
        return context.AudioOptions;
    }

    // GET: api/AudioOptions/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAudioOptions([FromRoute] int id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var audioOptions = await context.AudioOptions.FindAsync(id);

        if (audioOptions == null) return NotFound();

        return Ok(audioOptions);
    }

    // PUT: api/AudioOptions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAudioOptions([FromRoute] int id, [FromBody] AudioOptions audioOptions)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id != audioOptions.AudioId) return BadRequest();

        context.Entry(audioOptions).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AudioOptionsExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // POST: api/AudioOptions
    [HttpPost]
    public async Task<IActionResult> PostAudioOptions([FromBody] AudioOptions audioOptions)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        context.AudioOptions.Add(audioOptions);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetAudioOptions", new { id = audioOptions.AudioId }, audioOptions);
    }

    // DELETE: api/AudioOptions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAudioOptions([FromRoute] int id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var audioOptions = await context.AudioOptions.FindAsync(id);
        if (audioOptions == null) return NotFound();

        context.AudioOptions.Remove(audioOptions);
        await context.SaveChangesAsync();

        return Ok(audioOptions);
    }

    private bool AudioOptionsExists(int id)
    {
        return context.AudioOptions.Any(e => e.AudioId == id);
    }
}