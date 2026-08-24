using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSync.Data;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
public class TeamRadarController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeamRadarController(ApplicationDbContext context)
    {
        _context = context;
    }

    // TODO: Add team-level radar once Team -> Employee relationship is available from the database schema.
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeRadar(string employeeId)
    {
        var result = await _context.EmployeeSkills
            .Where(x => x.Employee.ApplicationUserId == employeeId)
            .Include(x => x.Skill)
            .GroupBy(x => x.Skill.Name)
            .Select(g => new
            {
                Skill = g.Key,
                AverageScore = g.Average(x => x.Score)
            })
            .ToListAsync();

        return Ok(result);
    }
}