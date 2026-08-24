using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillRadarReports.Dtos;
using SkillRadarReports.Services;

namespace SkillRadarReports.Controllers;

[ApiController]
[Route("api/allocations")]
public class AllocationsController : ControllerBase
{
    private readonly IAllocationService _allocationService;

    public AllocationsController(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [HttpPost]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<AllocationResponseDto>> CreateAllocation([FromBody] CreateAllocationDto dto)
    {
        try
        {
            var result = await _allocationService.CreateAllocationAsync(dto);
            return CreatedAtAction(nameof(GetConflicts), new { employeeId = result.EmployeeId }, result);
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

    [HttpPut("{id}")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<AllocationResponseDto>> UpdateAllocation(int id, [FromBody] UpdateAllocationDto dto)
    {
        try
        {
            var result = await _allocationService.UpdateAllocationAsync(id, dto);
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

    [HttpGet("{employeeId}/conflicts")]
    [Authorize(Roles = "ProjectManager,ResourceManager")]
    public async Task<ActionResult<AllocationConflictDto>> GetConflicts(int employeeId)
    {
        try
        {
            var conflicts = await _allocationService.GetEmployeeConflictsAsync(employeeId);
            return Ok(conflicts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/lock")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<AllocationResponseDto>> LockAllocation(int id)
    {
        try
        {
            var result = await _allocationService.LockAllocationAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<IActionResult> DeleteAllocation(int id)
    {
        try
        {
            var deleted = await _allocationService.DeleteAllocationAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Allocation with ID {id} not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
