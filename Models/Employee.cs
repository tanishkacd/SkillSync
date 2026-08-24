namespace SkillSync.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;

        public int DepartmentID { get; set; }

        public Department Department { get; set; } = null!;

        public ICollection<EmployeeSkill> EmployeeSkills { get; set; }
            = new List<EmployeeSkill>();

        public ICollection<EmployeeCertification> EmployeeCertifications { get; set; }
            = new List<EmployeeCertification>();
    }
}