namespace SkillRadarReports.Dtos;

public class TimesheetEntryDto
{
    public int TimesheetEntryID { get; set; }
    public int TimesheetID { get; set; }
    public int ProjectID { get; set; }
    public string? ProjectName { get; set; }
    public int? TaskID { get; set; }
    public string? TaskName { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Notes { get; set; }
}

public class TimesheetDto
{
    public int TimesheetID { get; set; }
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime WeekStartDate { get; set; }
    public string Status { get; set; } = "Draft";
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public decimal TotalHours { get; set; }
    public List<TimesheetEntryDto> Entries { get; set; } = new();
}

public class SaveTimesheetEntryDto
{
    public int? TimesheetEntryID { get; set; }
    public int ProjectID { get; set; }
    public int? TaskID { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Notes { get; set; }
}

public class SaveTimesheetDto
{
    public int EmployeeID { get; set; }
    public DateTime WeekStartDate { get; set; }
    public List<SaveTimesheetEntryDto> Entries { get; set; } = new();
}

public class RejectTimesheetDto
{
    public string Reason { get; set; } = string.Empty;
}
