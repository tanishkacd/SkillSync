using SkillRadarReports.Dtos;

namespace SkillRadarReports.Services;

public interface IAllocationService
{
    Task<AllocationResponseDto> CreateAllocationAsync(CreateAllocationDto dto);
    Task<AllocationResponseDto> UpdateAllocationAsync(int allocationId, UpdateAllocationDto dto);
    Task<AllocationConflictDto> GetEmployeeConflictsAsync(int employeeId);
    Task<AllocationResponseDto> LockAllocationAsync(int allocationId);
    Task<bool> DeleteAllocationAsync(int allocationId);
}
