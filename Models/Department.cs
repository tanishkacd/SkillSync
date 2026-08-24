namespace SkillSync.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}
