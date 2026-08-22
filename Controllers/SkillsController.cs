using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSync.Data;
using SkillSync.Models;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SkillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _context.Skills
            .Include(s => s.SkillCategory)
            .Select(s => new
            {
                s.Id,
                s.Name,
                Category = s.SkillCategory.Name
            })
            .ToListAsync();

        return Ok(skills);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSkill(int id)
    {
        var skill = await _context.Skills
            .Include(s => s.SkillCategory)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Name,
                Category = s.SkillCategory.Name
            })
            .FirstOrDefaultAsync();

        return skill is null ? NotFound() : Ok(skill);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill(Skill skill)
    
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
    }
}
