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
    public class SetSequencesController : ControllerBase
    {
        private readonly EfHolidayContext _context;

        public SetSequencesController(EfHolidayContext context)
        {
            _context = context;
        }

        // GET: api/SetSequences
        [HttpGet]
        public IEnumerable<SetSequences> GetSetSequences()
        {
            return _context.SetSequences;
        }

        // GET: api/SetSequences/5
        [HttpGet("SetSequencesBySetId/{setId}")]
        public IEnumerable<SetSequences> GetSetSequencesBySetId([FromRoute] int setId)
        {
            return _context.SetSequences.Where(x => x.SetId == setId);
        }

        // GET: api/SetSequences/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSetSequences([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var setSequences = await _context.SetSequences.FindAsync(id);

            if (setSequences == null)
            {
                return NotFound();
            }

            return Ok(setSequences);
        }

        // PUT: api/SetSequences/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSetSequences([FromRoute] int id, [FromBody] SetSequences setSequences)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != setSequences.SetSequenceId)
            {
                return BadRequest();
            }

            _context.Entry(setSequences).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SetSequencesExists(id))
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

        // POST: api/SetSequences
        [HttpPost]
        public async Task<IActionResult> PostSetSequences([FromBody] SetSequences setSequences)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SetSequences.Add(setSequences);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSetSequences", new { id = setSequences.SetSequenceId }, setSequences);
        }

        // DELETE: api/SetSequences/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSetSequences([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var setSequences = await _context.SetSequences.FindAsync(id);
            if (setSequences == null)
            {
                return NotFound();
            }

            _context.SetSequences.Remove(setSequences);
            await _context.SaveChangesAsync();

            return Ok(setSequences);
        }

        private bool SetSequencesExists(int id)
        {
            return _context.SetSequences.Any(e => e.SetSequenceId == id);
        }
    }
}