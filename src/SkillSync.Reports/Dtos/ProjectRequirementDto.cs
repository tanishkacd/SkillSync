using System.ComponentModel.DataAnnotations;

namespace SkillRadarReports.Dtos;

public class CreateProjectRequirementDto
{
    [Required]
    public int SkillId { get; set; }

    [Range(0, 5, ErrorMessage = "MinProficiency must be between 0 and 5.")]
    public byte MinProficiency { get; set; }

    [Range(0, 100, ErrorMessage = "MinYearsExperience must be greater than or equal to 0.")]
    public decimal MinYearsExperience { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "HeadcountNeeded must be at least 1.")]
    public int HeadcountNeeded { get; set; }
}

public class UpdateProjectRequirementDto
{
    [Range(0, 5, ErrorMessage = "MinProficiency must be between 0 and 5.")]
    public byte MinProficiency { get; set; }

    [Range(0, 100, ErrorMessage = "MinYearsExperience must be greater than or equal to 0.")]
    public decimal MinYearsExperience { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "HeadcountNeeded must be at least 1.")]
    public int HeadcountNeeded { get; set; }
}

public class ProjectRequirementResponseDto
{
    public int ProjectRequirementId { get; set; }
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public byte MinProficiency { get; set; }
    public decimal MinYearsExperience { get; set; }
    public int HeadcountNeeded { get; set; }
}
