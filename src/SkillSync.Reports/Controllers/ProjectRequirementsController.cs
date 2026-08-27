using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillRadarReports.Dtos;
using SkillRadarReports.Services;

namespace SkillRadarReports.Controllers;

[ApiController]
[Route("api/projects/{projectId}/requirements")]
public class ProjectRequirementsController : ControllerBase
{
    private readonly IProjectRequirementService _requirementService;

    public ProjectRequirementsController(IProjectRequirementService requirementService)
    {
        _requirementService = requirementService;
    }

    [HttpPost]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<ProjectRequirementResponseDto>> AddRequirement(int projectId, [FromBody] CreateProjectRequirementDto dto)
    {
        try
        {
            var result = await _requirementService.AddRequirementAsync(projectId, dto);
            return CreatedAtAction(nameof(GetRequirements), new { projectId = result.ProjectId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "ProjectManager,ResourceManager")]
    public async Task<ActionResult<List<ProjectRequirementResponseDto>>> GetRequirements(int projectId)
    {
        try
        {
            var results = await _requirementService.GetRequirementsByProjectIdAsync(projectId);
            return Ok(results);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<ProjectRequirementResponseDto>> UpdateRequirement(int projectId, int id, [FromBody] UpdateProjectRequirementDto dto)
    {
        try
        {
            var result = await _requirementService.UpdateRequirementAsync(projectId, id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<IActionResult> DeleteRequirement(int projectId, int id)
    {
        var deleted = await _requirementService.DeleteRequirementAsync(projectId, id);
        if (!deleted)
        {
            return NotFound(new { message = $"Project requirement {id} for project {projectId} not found." });
        }

        return NoContent();
    }
}
