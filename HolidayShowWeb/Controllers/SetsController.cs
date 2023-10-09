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
    public class SetsController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public SetsController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/Sets
        [HttpGet]
        public IEnumerable<Sets> GetSets()
        {
            return _context.Sets;
        }

        // GET: api/Sets/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSets([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sets = await _context.Sets.FindAsync(id);

            if (sets == null)
            {
                return NotFound();
            }

            return Ok(sets);
        }

        // PUT: api/Sets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSets([FromRoute] int id, [FromBody] Sets sets)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sets.SetId)
            {
                return BadRequest();
            }

            _context.Entry(sets).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SetsExists(id))
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

        // POST: api/Sets
        [HttpPost]
        public async Task<IActionResult> PostSets([FromBody] Sets sets)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Sets.Add(sets);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSets", new { id = sets.SetId }, sets);
        }

        // DELETE: api/Sets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSets([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sets = await _context.Sets.FindAsync(id);
            if (sets == null)
            {
                return NotFound();
            }

            _context.Sets.Remove(sets);
            await _context.SaveChangesAsync();

            return Ok(sets);
        }

        private bool SetsExists(int id)
        {
            return _context.Sets.Any(e => e.SetId == id);
        }
    }
}