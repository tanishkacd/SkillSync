using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;

namespace SkillRadarReports.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MilestonesController : ControllerBase
{
    private readonly AppDbContext _db;

    public MilestonesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<MilestoneDto>>> GetAll([FromQuery] int? projectId)
    {
        var query = _db.Milestones.AsQueryable();
        if (projectId.HasValue)
            query = query.Where(m => m.ProjectId == projectId.Value);

        var milestones = await query
            .Select(m => new MilestoneDto
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                Title = m.Title,
                DueDate = m.DueDate,
                Status = m.Status,
                PercentComplete = m.PercentComplete,
                IsDelayed = m.IsDelayed
            })
            .ToListAsync();

        return Ok(milestones);
    }

    [HttpPost]
    public async Task<ActionResult<MilestoneDto>> Create(CreateMilestoneDto dto)
    {
        var milestone = new Milestone
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            DueDate = dto.DueDate,
            Status = MilestoneStatus.NotStarted,
            PercentComplete = 0
        };

        _db.Milestones.Add(milestone);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { projectId = milestone.ProjectId }, milestone);
    }

    [HttpPut("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, UpdateMilestoneProgressDto dto)
    {
        var milestone = await _db.Milestones.FindAsync(id);
        if (milestone is null) return NotFound();

        milestone.PercentComplete = dto.PercentComplete;
        milestone.Status = dto.Status;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
