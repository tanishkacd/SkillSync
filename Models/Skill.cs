using System.Collections.Generic;

namespace SkillSync.Models
{
    public class Skill
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int SkillCategoryId { get; set; }

        public SkillCategory SkillCategory { get; set; } = null!;

        public ICollection<EmployeeSkill> EmployeeSkills { get; set; }
            = new List<EmployeeSkill>();
    }
}