namespace SkillRadarReports.Models;

public class Timesheet
{
    public int TimesheetID { get; set; }
    public int EmployeeID { get; set; }
    public DateTime WeekStartDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft / Submitted / Approved / Rejected
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public Employee? Employee { get; set; }
    public Employee? Approver { get; set; }
    public ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
}
