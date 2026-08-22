namespace SkillSync.Models;

public class EmployeeCertification
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;
    public ApplicationUser Employee { get; set; } = null!;

    public int CertificationId { get; set; }
    public Certification Certification { get; set; } = null!;

    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CredentialId { get; set; }
}
