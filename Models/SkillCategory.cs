namespace SkillSync.Models;

public class SkillCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
