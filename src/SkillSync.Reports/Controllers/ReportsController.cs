using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using SkillRadarReports.Dtos;
using System.Globalization;

namespace SkillRadarReports.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    // TODO: once Person 6's Allocation/Timesheet tables exist, replace this stub
    // with a real EF Core query (ideally against the reporting VIEW Person 1+2 build).
    [HttpGet("availability")]
    public ActionResult<List<AvailabilityReportDto>> GetAvailability(
        [FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var rangeStart = start ?? DateTime.UtcNow.Date;
        var rangeEnd = end ?? rangeStart.AddDays(7);

        // Placeholder demo data so the endpoint is testable right now.
        var demo = new List<AvailabilityReportDto>
        {
            new() { EmployeeId = 1, EmployeeName = "Aisha Khan", RangeStart = rangeStart, RangeEnd = rangeEnd, TotalAvailableHours = 40, BookedHours = 32 },
            new() { EmployeeId = 2, EmployeeName = "Rohan Mehta", RangeStart = rangeStart, RangeEnd = rangeEnd, TotalAvailableHours = 40, BookedHours = 40 },
            new() { EmployeeId = 3, EmployeeName = "Priya Nair", RangeStart = rangeStart, RangeEnd = rangeEnd, TotalAvailableHours = 40, BookedHours = 12 },
        };

        return Ok(demo);
    }

    // TODO: replace with a real query joining Project budget vs sum(Timesheet.hours * Employee.rate)
    [HttpGet("profitability")]
    public ActionResult<List<ProfitabilityReportDto>> GetProfitability()
    {
        var demo = new List<ProfitabilityReportDto>
        {
            new() { ProjectId = 101, ProjectName = "SkillRadar Internal Rollout", Budget = 500000, TotalHoursLogged = 620, TotalCost = 372000 },
            new() { ProjectId = 102, ProjectName = "Client Portal Revamp", Budget = 250000, TotalHoursLogged = 410, TotalCost = 246000 },
        };

        return Ok(demo);
    }

    [HttpGet("availability/export")]
    public IActionResult ExportAvailabilityCsv([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var data = GetAvailability(start, end).Value ?? new List<AvailabilityReportDto>();

        using var writer = new StringWriter();
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data);
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(writer.ToString());
        return File(bytes, "text/csv", "availability_report.csv");
    }

    [HttpGet("profitability/export")]
    public IActionResult ExportProfitabilityCsv()
    {
        var data = GetProfitability().Value ?? new List<ProfitabilityReportDto>();

        using var writer = new StringWriter();
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data);
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(writer.ToString());
        return File(bytes, "text/csv", "profitability_report.csv");
    }
}
