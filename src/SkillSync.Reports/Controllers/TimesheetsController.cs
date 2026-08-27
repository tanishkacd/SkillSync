using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillRadarReports.Dtos;
using SkillRadarReports.Services;

namespace SkillRadarReports.Controllers;

[ApiController]
[Route("api/timesheets")]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;

    public TimesheetsController(ITimesheetService timesheetService)
    {
        _timesheetService = timesheetService;
    }

    [HttpGet("{employeeId}/week/{weekStartDate}")]
    [Authorize(Roles = "Employee,ProjectManager,ResourceManager,HRAdmin")]
    public async Task<ActionResult<TimesheetDto>> GetTimesheet(int employeeId, DateTime weekStartDate)
    {
        try
        {
            var result = await _timesheetService.GetOrInitializeTimesheetAsync(employeeId, weekStartDate);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<TimesheetDto>> SaveTimesheet([FromBody] SaveTimesheetDto dto)
    {
        try
        {
            var result = await _timesheetService.SaveTimesheetAsync(dto);
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

    [HttpPut("{id}/submit")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<TimesheetDto>> SubmitTimesheet(int id)
    {
        try
        {
            var result = await _timesheetService.SubmitTimesheetAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<TimesheetDto>> ApproveTimesheet(int id, [FromQuery] int managerEmployeeId)
    {
        try
        {
            var result = await _timesheetService.ApproveTimesheetAsync(id, managerEmployeeId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "ProjectManager")]
    public async Task<ActionResult<TimesheetDto>> RejectTimesheet(int id, [FromQuery] int managerEmployeeId, [FromBody] RejectTimesheetDto dto)
    {
        try
        {
            var result = await _timesheetService.RejectTimesheetAsync(id, managerEmployeeId, dto?.Reason ?? string.Empty);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
