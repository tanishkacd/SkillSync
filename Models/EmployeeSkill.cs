namespace SkillSync.Models
{
    public class EmployeeSkill
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public int SkillId { get; set; }

        public Skill Skill { get; set; } = null!;

        public decimal Score { get; set; }

        public DateTime? LastAssessedDate { get; set; }
    }
}