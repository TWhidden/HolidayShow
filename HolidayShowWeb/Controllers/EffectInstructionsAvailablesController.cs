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
    public class EffectInstructionsAvailablesController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public EffectInstructionsAvailablesController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/EffectInstructionsAvailables
        [HttpGet]
        public IEnumerable<EffectInstructionsAvailable> GetEffectInstructionsAvailable()
        {
            return _context.EffectInstructionsAvailable;
        }

        // GET: api/EffectInstructionsAvailables/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEffectInstructionsAvailable([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var effectInstructionsAvailable = await _context.EffectInstructionsAvailable.FindAsync(id);

            if (effectInstructionsAvailable == null)
            {
                return NotFound();
            }

            return Ok(effectInstructionsAvailable);
        }

        // PUT: api/EffectInstructionsAvailables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEffectInstructionsAvailable([FromRoute] int id, [FromBody] EffectInstructionsAvailable effectInstructionsAvailable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != effectInstructionsAvailable.EffectInstructionId)
            {
                return BadRequest();
            }

            _context.Entry(effectInstructionsAvailable).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EffectInstructionsAvailableExists(id))
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

        // POST: api/EffectInstructionsAvailables
        [HttpPost]
        public async Task<IActionResult> PostEffectInstructionsAvailable([FromBody] EffectInstructionsAvailable effectInstructionsAvailable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.EffectInstructionsAvailable.Add(effectInstructionsAvailable);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEffectInstructionsAvailable", new { id = effectInstructionsAvailable.EffectInstructionId }, effectInstructionsAvailable);
        }

        // DELETE: api/EffectInstructionsAvailables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEffectInstructionsAvailable([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var effectInstructionsAvailable = await _context.EffectInstructionsAvailable.FindAsync(id);
            if (effectInstructionsAvailable == null)
            {
                return NotFound();
            }

            _context.EffectInstructionsAvailable.Remove(effectInstructionsAvailable);
            await _context.SaveChangesAsync();

            return Ok(effectInstructionsAvailable);
        }

        private bool EffectInstructionsAvailableExists(int id)
        {
            return _context.EffectInstructionsAvailable.Any(e => e.EffectInstructionId == id);
        }
    }
}