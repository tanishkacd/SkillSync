namespace SkillRadarReports.Dtos;

// One row per project. Cost = sum(hours logged * employee hourly rate).
// Revenue/Budget will come from the Project table once Person 1+2's schema is finalized.
public class ProfitabilityReportDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public double TotalHoursLogged { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Margin => Budget - TotalCost;
    public double MarginPercent => Budget == 0 ? 0 : (double)(Margin / Budget * 100);
}
