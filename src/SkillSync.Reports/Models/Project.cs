namespace SkillRadarReports.Models;

public class Project
{
    public int ProjectID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public int ProjectManagerID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Planning";
    public decimal? BudgetAmount { get; set; }
    public decimal? BillingRatePerHour { get; set; }

    public Employee? ProjectManager { get; set; }
    public ICollection<ProjectRequirement> Requirements { get; set; } = new List<ProjectRequirement>();
    public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
}
