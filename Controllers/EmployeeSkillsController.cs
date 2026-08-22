using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSync.Data;
using SkillSync.Models;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeSkillsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeeSkillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{employeeId}")]
    [Authorize]
    public async Task<IActionResult> GetEmployeeSkills(string employeeId)
    {
        var skills = await _context.EmployeeSkills
            .Where(x => x.EmployeeId == employeeId)
            .Include(x => x.Skill)
            .Include(x => x.Certification)
            .Select(x => new
            {
                x.Id,
                x.EmployeeId,
                Skill = x.Skill.Name,
                x.Score,
                x.CertificationId,
                Certification = x.Certification == null ? null : x.Certification.Name,
                x.LastAssessedDate
            })
            .ToListAsync();

        return Ok(skills);
    }

    [HttpPut("{id:int}/score")]
    [Authorize(Roles = "HR Administrator")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] decimal score)
    {
        if (score < 0 || score > 5)
            return BadRequest("Skill score must be between 0 and 5.");

        var employeeSkill = await _context.EmployeeSkills.FindAsync(id);

        if (employeeSkill is null)
            return NotFound();

        employeeSkill.Score = score;
        employeeSkill.LastAssessedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(employeeSkill);
    }
}
