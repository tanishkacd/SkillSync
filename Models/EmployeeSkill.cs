namespace SkillSync.Models;

public class EmployeeSkill
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;
    public ApplicationUser Employee { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public decimal Score { get; set; }
    public int? CertificationId { get; set; }
    public Certification? Certification { get; set; }
    public DateTime LastAssessedDate { get; set; }
}
