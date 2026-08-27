namespace SkillRadarReports.Models;

public enum NotificationType
{
    MilestoneOverdue,
    TimesheetApproved,
    TimesheetRejected,
    NewAllocation,
    General
}

// NOTE: UserId is a plain string placeholder matching ASP.NET Identity's default key type.
// Once Person 3+4's Identity/roles setup is merged, this will map to IdentityUser.Id.
public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
