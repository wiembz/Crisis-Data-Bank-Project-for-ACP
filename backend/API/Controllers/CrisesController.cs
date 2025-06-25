using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CrisesController : ControllerBase
{
    private readonly CrisisDbContext _context;

    public CrisesController(CrisisDbContext context) => _context = context;

    // GET: api/crises
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Crisis>>> GetAll()
    {
        return await _context.Crises.ToListAsync();
    }

    // GET: api/crises/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Crisis>> GetById(int id)
    {
        var crisis = await _context.Crises.FindAsync(id);

        if (crisis == null)
        {
            return NotFound();
        }

        return crisis;
    }

    // GET: api/crises/search?query=something
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Crisis>>> Search(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return await _context.Crises.ToListAsync();
        }

        // Get all crises first
        var allCrises = await _context.Crises.ToListAsync();
        
        // Convert query to lowercase for case-insensitive comparison
        query = query.ToLower();
        
        // Use LINQ to filter crises in-memory where we have full support for 
        // complex operations on deserialized objects
        var filteredCrises = allCrises.Where(c => 
            c.Title?.ToLower().Contains(query) == true ||
            c.Description?.ToLower().Contains(query) == true ||
            c.ReportedBy?.ToLower().Contains(query) == true ||
            c.AssignedTo?.ToLower().Contains(query) == true ||
            c.Resolution?.ToLower().Contains(query) == true ||
            (c.Tags != null && c.Tags.Any(t => t.ToLower().Contains(query))) ||
            (c.AffectedSystems != null && c.AffectedSystems.Any(s => s.ToLower().Contains(query)))
        ).ToList();
        
        return filteredCrises;
    }

    // GET: api/crises/filter?severity=High&status=Open
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<Crisis>>> Filter(string? severity, string? status)
    {
        // Get all crises
        var crises = await _context.Crises.ToListAsync();
        
        // Apply filters in-memory for more reliable filtering
        var filteredCrises = crises.AsEnumerable();
        
        if (!string.IsNullOrEmpty(severity))
        {
            filteredCrises = filteredCrises.Where(c => c.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(status))
        {
            filteredCrises = filteredCrises.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return filteredCrises.ToList();
    }

    // GET: api/crises/statistics
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetStatistics()
    {
        var crises = await _context.Crises.ToListAsync();
        
        var statistics = new
        {
            Total = crises.Count,
            BySeverity = new
            {
                Low = crises.Count(c => c.Severity == "Low"),
                Medium = crises.Count(c => c.Severity == "Medium"),
                High = crises.Count(c => c.Severity == "High"),
                Critical = crises.Count(c => c.Severity == "Critical")
            },
            ByStatus = new
            {
                Open = crises.Count(c => c.Status == "Open"),
                InProgress = crises.Count(c => c.Status == "In Progress"),
                Resolved = crises.Count(c => c.Status == "Resolved")
            },
            RecentlyReported = crises.OrderByDescending(c => c.DateReported)
                                    .Take(5)
                                    .Select(c => new { c.Id, c.Title, c.Severity, c.Status, c.DateReported })
                                    .ToList(),
            RecentlyResolved = crises.Where(c => c.DateResolved != null)
                                   .OrderByDescending(c => c.DateResolved)
                                   .Take(5)
                                   .Select(c => new { c.Id, c.Title, c.Severity, c.Status, c.DateResolved })
                                   .ToList(),
            TopTags = crises.Where(c => c.Tags != null && c.Tags.Any())
                           .SelectMany(c => c.Tags!)
                           .GroupBy(t => t)
                           .Select(g => new { Tag = g.Key, Count = g.Count() })
                           .OrderByDescending(x => x.Count)
                           .Take(10)
                           .ToList()
        };

        return statistics;
    }

    // POST: api/crises
    [HttpPost]
    public async Task<ActionResult<Crisis>> Create(Crisis crisis)
    {
        crisis.DateReported = DateTime.Now;
        
        if (crisis.Status == "Resolved" && crisis.DateResolved == null)
        {
            crisis.DateResolved = DateTime.Now;
        }

        _context.Crises.Add(crisis);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = crisis.Id }, crisis);
    }

    // PUT: api/crises/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Crisis crisis)
    {
        if (id != crisis.Id)
        {
            return BadRequest();
        }

        // If status is being set to Resolved and there's no resolution date yet, set it now
        if (crisis.Status == "Resolved" && crisis.DateResolved == null)
        {
            crisis.DateResolved = DateTime.Now;
        }

        // If status is not Resolved, clear the resolution date
        if (crisis.Status != "Resolved")
        {
            crisis.DateResolved = null;
        }

        _context.Entry(crisis).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CrisisExists(id))
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

    // DELETE: api/crises/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var crisis = await _context.Crises.FindAsync(id);
        if (crisis == null)
        {
            return NotFound();
        }

        _context.Crises.Remove(crisis);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CrisisExists(int id)
    {
        return _context.Crises.Any(e => e.Id == id);
    }
}
