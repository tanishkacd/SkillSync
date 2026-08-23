using SkillRadarReports.Models;

namespace SkillRadarReports.Dtos;

public class MilestoneDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public int PercentComplete { get; set; }
    public bool IsDelayed { get; set; }
}

public class CreateMilestoneDto
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

public class UpdateMilestoneProgressDto
{
    public int PercentComplete { get; set; }
    public MilestoneStatus Status { get; set; }
}
