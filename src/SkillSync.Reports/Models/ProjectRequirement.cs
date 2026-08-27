namespace SkillRadarReports.Models;

public class ProjectRequirement
{
    public int ProjectRequirementID { get; set; }
    public int ProjectID { get; set; }
    public int SkillID { get; set; }
    public byte MinProficiency { get; set; }
    public decimal MinYearsExperience { get; set; }
    public int HeadcountNeeded { get; set; }

    public Project? Project { get; set; }
    public Skill? Skill { get; set; }
}
