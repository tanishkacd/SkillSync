namespace SkillRadarReports.Models;

public enum MilestoneStatus
{
    NotStarted,
    InProgress,
    Completed,
    Delayed
}

// NOTE: ProjectId is a plain int/foreign key placeholder for now.
// Once Person 1+2's Project table exists, point this FK at their real Projects table
// (add a navigation property `public Project Project { get; set; }` once merged).
public class Milestone
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;
    public int PercentComplete { get; set; } // 0-100

    // Convenience: is this milestone overdue right now?
    public bool IsDelayed => Status != MilestoneStatus.Completed && DueDate < DateTime.UtcNow;
}
