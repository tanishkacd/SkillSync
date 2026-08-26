using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSync.Data;
using SkillSync.Models;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeSkillsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeeSkillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/EmployeeSkills
    [HttpGet]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<IActionResult> GetEmployeeSkills()
    {
        var employeeSkills = await _context.EmployeeSkills
            .Select(es => new
            {
                id = es.Id,
                employeeId = es.EmployeeId,
                skillId = es.SkillId,
                score = es.Score,
                lastAssessedDate = es.LastAssessedDate,
                employeeName = es.Employee != null
                    ? es.Employee.ApplicationUser.FullName
                    : null,
                skillName = es.Skill != null
                    ? es.Skill.Name
                    : null
            })
            .ToListAsync();

        return Ok(employeeSkills);
    }

    // GET: api/EmployeeSkills/5
    [HttpGet("{id}")]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<ActionResult<EmployeeSkill>> GetEmployeeSkill(int id)
    {
        var employeeSkill = await _context.EmployeeSkills
            .Include(es => es.Employee)
            .Include(es => es.Skill)
            .FirstOrDefaultAsync(es => es.Id == id);

        if (employeeSkill == null)
        {
            return NotFound(new
            {
                message = "Employee skill not found."
            });
        }

        return Ok(employeeSkill);
    }

    // GET: api/EmployeeSkills/employee/5
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<ActionResult<IEnumerable<EmployeeSkill>>> GetEmployeeSkillsByEmployee(
        int employeeId)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId);

        if (!employeeExists)
        {
            return NotFound(new
            {
                message = "Employee not found."
            });
        }

        var employeeSkills = await _context.EmployeeSkills
            .Where(es => es.EmployeeId == employeeId)
            .Include(es => es.Employee)
            .Include(es => es.Skill)
            .ToListAsync();

        return Ok(employeeSkills);
    }

    // GET: api/EmployeeSkills/skill/5
    [HttpGet("skill/{skillId}")]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<ActionResult<IEnumerable<EmployeeSkill>>> GetEmployeesBySkill(
        int skillId)
    {
        var skillExists = await _context.Skills
            .AnyAsync(s => s.Id == skillId);

        if (!skillExists)
        {
            return NotFound(new
            {
                message = "Skill not found."
            });
        }

        var employeeSkills = await _context.EmployeeSkills
            .Where(es => es.SkillId == skillId)
            .Include(es => es.Employee)
            .Include(es => es.Skill)
            .ToListAsync();

        return Ok(employeeSkills);
    }

    // GET: api/EmployeeSkills/department/5/radar
    [HttpGet("department/{departmentId}/radar")]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<IActionResult> GetDepartmentSkillRadar(int departmentId)
    {
        // Check whether the department exists
        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentID == departmentId);

        if (!departmentExists)
        {
            return NotFound(new
            {
                message = "Department not found."
            });
        }

        // Get average score of each skill for employees
        // belonging to the selected department
        var radarData = await _context.EmployeeSkills
            .Where(es => es.Employee != null &&
             es.Employee.DepartmentID == departmentId)
            .GroupBy(es => new
            {
                es.SkillId,
                SkillName = es.Skill != null ? es.Skill.Name : ""
            })
            .Select(g => new
            {
                skillId = g.Key.SkillId,
                skill = g.Key.SkillName,
                averageScore = Math.Round(g.Average(es => es.Score), 2)
            })
            .OrderBy(x => x.skill)
            .ToListAsync();

        return Ok(radarData);
    }

    // POST: api/EmployeeSkills
    [HttpPost]
    [Authorize(Roles = "HR Administrator,System Administrator")]
    public async Task<ActionResult<EmployeeSkill>> CreateEmployeeSkill(
        EmployeeSkill employeeSkill)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeSkill.EmployeeId);

        if (!employeeExists)
        {
            return BadRequest(new
            {
                message = "Employee does not exist."
            });
        }

        var skillExists = await _context.Skills
            .AnyAsync(s => s.Id == employeeSkill.SkillId);

        if (!skillExists)
        {
            return BadRequest(new
            {
                message = "Skill does not exist."
            });
        }

        var duplicate = await _context.EmployeeSkills
            .AnyAsync(es =>
                es.EmployeeId == employeeSkill.EmployeeId &&
                es.SkillId == employeeSkill.SkillId);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "This employee already has this skill."
            });
        }

        if (employeeSkill.Score < 0 || employeeSkill.Score > 5)
        {
            return BadRequest(new
            {
                message = "Score must be between 0 and 5."
            });
        }

        employeeSkill.LastAssessedDate = DateTime.UtcNow;

        _context.EmployeeSkills.Add(employeeSkill);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
     nameof(GetEmployeeSkill),
     new { id = employeeSkill.Id },
     new
     {
         id = employeeSkill.Id,
         employeeId = employeeSkill.EmployeeId,
         skillId = employeeSkill.SkillId,
         score = employeeSkill.Score,
         lastAssessedDate = employeeSkill.LastAssessedDate
     });
    }

    // PUT: api/EmployeeSkills/5
    [HttpPut("{id}")]
    [Authorize(Roles = "HR Administrator,System Administrator")]
    public async Task<IActionResult> UpdateEmployeeSkill(
        int id,
        EmployeeSkill employeeSkill)
    {
        if (id != employeeSkill.Id)
        {
            return BadRequest(new
            {
                message = "ID in URL does not match ID in request body."
            });
        }

        var existingEmployeeSkill = await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.Id == id);

        if (existingEmployeeSkill == null)
        {
            return NotFound(new
            {
                message = "Employee skill not found."
            });
        }

        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeSkill.EmployeeId);

        if (!employeeExists)
        {
            return BadRequest(new
            {
                message = "Employee does not exist."
            });
        }

        var skillExists = await _context.Skills
            .AnyAsync(s => s.Id == employeeSkill.SkillId);

        if (!skillExists)
        {
            return BadRequest(new
            {
                message = "Skill does not exist."
            });
        }

        if (employeeSkill.Score < 0 || employeeSkill.Score > 5)
        {
            return BadRequest(new
            {
                message = "Score must be between 0 and 5."
            });
        }

        var duplicate = await _context.EmployeeSkills
            .AnyAsync(es =>
                es.Id != id &&
                es.EmployeeId == employeeSkill.EmployeeId &&
                es.SkillId == employeeSkill.SkillId);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "This employee already has this skill."
            });
        }

        existingEmployeeSkill.EmployeeId = employeeSkill.EmployeeId;
        existingEmployeeSkill.SkillId = employeeSkill.SkillId;
        existingEmployeeSkill.Score = employeeSkill.Score;
        existingEmployeeSkill.LastAssessedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/EmployeeSkills/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "HR Administrator,System Administrator")]
    public async Task<IActionResult> DeleteEmployeeSkill(int id)
    {
        var employeeSkill = await _context.EmployeeSkills
            .FindAsync(id);

        if (employeeSkill == null)
        {
            return NotFound(new
            {
                message = "Employee skill not found."
            });
        }

        _context.EmployeeSkills.Remove(employeeSkill);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
