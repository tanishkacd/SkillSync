namespace SkillRadarReports.Models;

public class Allocation
{
    public int AllocationID { get; set; }
    public int ProjectID { get; set; }
    public int EmployeeID { get; set; }
    public int? ProjectRequirementID { get; set; }
    public decimal AllocationPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Proposed"; // Proposed / Locked / Completed / Cancelled
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public Employee? Employee { get; set; }
    public ProjectRequirement? ProjectRequirement { get; set; }
    public Employee? Creator { get; set; }
}
