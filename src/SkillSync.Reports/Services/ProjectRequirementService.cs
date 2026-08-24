using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;

namespace SkillRadarReports.Services;

public class ProjectRequirementService : IProjectRequirementService
{
    private readonly AppDbContext _db;

    public ProjectRequirementService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectRequirementResponseDto> AddRequirementAsync(int projectId, CreateProjectRequirementDto dto)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.ProjectID == projectId);
        if (!projectExists)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} was not found.");
        }

        var skill = await _db.Skills.FindAsync(dto.SkillId);
        if (skill is null)
        {
            throw new ArgumentException($"Skill with ID {dto.SkillId} does not exist.");
        }

        // Duplicate check: check if requirement for this (ProjectId, SkillId) already exists
        var existing = await _db.ProjectRequirements
            .FirstOrDefaultAsync(r => r.ProjectID == projectId && r.SkillID == dto.SkillId);

        if (existing is not null)
        {
            throw new InvalidOperationException($"A requirement for Skill {dto.SkillId} already exists in Project {projectId}. Use PUT to update the existing requirement.");
        }

        var newReq = new ProjectRequirement
        {
            ProjectID = projectId,
            SkillID = dto.SkillId,
            MinProficiency = dto.MinProficiency,
            MinYearsExperience = dto.MinYearsExperience,
            HeadcountNeeded = dto.HeadcountNeeded
        };

        _db.ProjectRequirements.Add(newReq);
        await _db.SaveChangesAsync();

        return MapToResponseDto(newReq, skill.Name);
    }

    public async Task<List<ProjectRequirementResponseDto>> GetRequirementsByProjectIdAsync(int projectId)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.ProjectID == projectId);
        if (!projectExists)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} was not found.");
        }

        var requirements = await _db.ProjectRequirements
            .Include(r => r.Skill)
            .Where(r => r.ProjectID == projectId)
            .ToListAsync();

        return requirements.Select(r => MapToResponseDto(r, r.Skill?.Name ?? string.Empty)).ToList();
    }

    public async Task<ProjectRequirementResponseDto> UpdateRequirementAsync(int projectId, int requirementId, UpdateProjectRequirementDto dto)
    {
        var requirement = await _db.ProjectRequirements
            .Include(r => r.Skill)
            .FirstOrDefaultAsync(r => r.ProjectRequirementID == requirementId && r.ProjectID == projectId);

        if (requirement is null)
        {
            throw new KeyNotFoundException($"Project requirement with ID {requirementId} was not found for project {projectId}.");
        }

        requirement.MinProficiency = dto.MinProficiency;
        requirement.MinYearsExperience = dto.MinYearsExperience;
        requirement.HeadcountNeeded = dto.HeadcountNeeded;

        await _db.SaveChangesAsync();

        return MapToResponseDto(requirement, requirement.Skill?.Name ?? string.Empty);
    }

    public async Task<bool> DeleteRequirementAsync(int projectId, int requirementId)
    {
        var requirement = await _db.ProjectRequirements
            .FirstOrDefaultAsync(r => r.ProjectRequirementID == requirementId && r.ProjectID == projectId);

        if (requirement is null)
        {
            return false;
        }

        _db.ProjectRequirements.Remove(requirement);
        await _db.SaveChangesAsync();

        return true;
    }

    private static ProjectRequirementResponseDto MapToResponseDto(ProjectRequirement req, string skillName)
    {
        return new ProjectRequirementResponseDto
        {
            ProjectRequirementId = req.ProjectRequirementID,
            ProjectId = req.ProjectID,
            SkillId = req.SkillID,
            SkillName = skillName,
            MinProficiency = req.MinProficiency,
            MinYearsExperience = req.MinYearsExperience,
            HeadcountNeeded = req.HeadcountNeeded
        };
    }
}
