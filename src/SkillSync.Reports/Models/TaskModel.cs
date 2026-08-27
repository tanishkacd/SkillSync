namespace SkillRadarReports.Models;

public class TaskModel
{
    public int TaskID { get; set; }
    public int MilestoneID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Not Started";

    public Milestone? Milestone { get; set; }
}
