namespace SkillSync.Models
{
    public class EmployeeCertification
    {
        public int EmployeeCertificationId { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public int CertificationId { get; set; }

        public Certification Certification { get; set; } = null!;

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}