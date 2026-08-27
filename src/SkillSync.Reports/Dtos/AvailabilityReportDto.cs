namespace SkillRadarReports.Dtos;

// One row per employee, for a given date range.
// BookedHours/TotalHours will eventually be computed from Person 6's Allocation + Timesheet tables.
public class AvailabilityReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }
    public double TotalAvailableHours { get; set; }
    public double BookedHours { get; set; }
    public double AvailabilityPercent =>
        TotalAvailableHours == 0 ? 0 : Math.Round(100 - (BookedHours / TotalAvailableHours * 100), 1);
}
