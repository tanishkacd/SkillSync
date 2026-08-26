namespace SkillRadarReports.Models;

public class TimesheetEntry
{
    public int TimesheetEntryID { get; set; }
    public int TimesheetID { get; set; }
    public int ProjectID { get; set; }
    public int? TaskID { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Notes { get; set; }

    public Timesheet? Timesheet { get; set; }
    public Project? Project { get; set; }
    public TaskModel? Task { get; set; }
}
