using SkillRadarReports.Dtos;

namespace SkillRadarReports.Services;

public interface IProjectRequirementService
{
    Task<ProjectRequirementResponseDto> AddRequirementAsync(int projectId, CreateProjectRequirementDto dto);
    Task<List<ProjectRequirementResponseDto>> GetRequirementsByProjectIdAsync(int projectId);
    Task<ProjectRequirementResponseDto> UpdateRequirementAsync(int projectId, int requirementId, UpdateProjectRequirementDto dto);
    Task<bool> DeleteRequirementAsync(int projectId, int requirementId);
}
