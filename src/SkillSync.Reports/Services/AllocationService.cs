using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;

namespace SkillRadarReports.Services;

public class AllocationService : IAllocationService
{
    private readonly AppDbContext _db;

    public AllocationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AllocationResponseDto> CreateAllocationAsync(CreateAllocationDto dto)
    {
        var project = await _db.Projects.FindAsync(dto.ProjectId);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with ID {dto.ProjectId} was not found.");
        }

        var employee = await _db.Employees.FindAsync(dto.EmployeeId);
        if (employee is null)
        {
            throw new KeyNotFoundException($"Employee with ID {dto.EmployeeId} was not found.");
        }

        if (dto.ProjectRequirementId.HasValue)
        {
            var reqExists = await _db.ProjectRequirements.AnyAsync(r => r.ProjectRequirementID == dto.ProjectRequirementId.Value && r.ProjectID == dto.ProjectId);
            if (!reqExists)
            {
                throw new ArgumentException($"ProjectRequirement with ID {dto.ProjectRequirementId.Value} does not belong to Project {dto.ProjectId}.");
            }
        }

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
        {
            throw new ArgumentException("EndDate cannot be earlier than StartDate.");
        }

        // Duplicate check: prevent POST from silently overwriting an existing active allocation
        var duplicate = await _db.Allocations
            .FirstOrDefaultAsync(a => a.ProjectID == dto.ProjectId &&
                                      a.EmployeeID == dto.EmployeeId &&
                                      a.StartDate.Date == dto.StartDate.Date &&
                                      a.Status != "Cancelled");

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"An active allocation already exists for Employee {dto.EmployeeId} on Project {dto.ProjectId} starting on {dto.StartDate:yyyy-MM-dd}. Use PUT /api/allocations/{duplicate.AllocationID} to modify it.");
        }

        // Validate capacity across overlapping date ranges
        await ValidateEmployeeCapacityAsync(dto.EmployeeId, null, dto.AllocationPercent, dto.StartDate, dto.EndDate);

        var allocation = new Allocation
        {
            ProjectID = dto.ProjectId,
            EmployeeID = dto.EmployeeId,
            ProjectRequirementID = dto.ProjectRequirementId,
            AllocationPercent = dto.AllocationPercent,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Proposed" : dto.Status,
            CreatedBy = dto.CreatedBy > 0 ? dto.CreatedBy : dto.EmployeeId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Allocations.Add(allocation);
        await _db.SaveChangesAsync();

        return MapToResponseDto(allocation, project.Name, $"{employee.FirstName} {employee.LastName}");
    }

    public async Task<AllocationResponseDto> UpdateAllocationAsync(int allocationId, UpdateAllocationDto dto)
    {
        var allocation = await _db.Allocations
            .Include(a => a.Project)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.AllocationID == allocationId);

        if (allocation is null)
        {
            throw new KeyNotFoundException($"Allocation with ID {allocationId} was not found.");
        }

        // Locking Enforcement: Cannot modify locked allocations
        if (allocation.Status == "Locked")
        {
            throw new InvalidOperationException($"Cannot modify allocation {allocationId} because it is locked.");
        }

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
        {
            throw new ArgumentException("EndDate cannot be earlier than StartDate.");
        }

        // Re-validate capacity excluding current allocation
        await ValidateEmployeeCapacityAsync(allocation.EmployeeID, allocation.AllocationID, dto.AllocationPercent, dto.StartDate, dto.EndDate);

        allocation.AllocationPercent = dto.AllocationPercent;
        allocation.StartDate = dto.StartDate;
        allocation.EndDate = dto.EndDate;
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            allocation.Status = dto.Status;
        }

        await _db.SaveChangesAsync();

        return MapToResponseDto(allocation, allocation.Project?.Name ?? string.Empty, $"{allocation.Employee?.FirstName} {allocation.Employee?.LastName}");
    }

    public async Task<AllocationConflictDto> GetEmployeeConflictsAsync(int employeeId)
    {
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee is null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} was not found.");
        }

        var activeAllocations = await _db.Allocations
            .Include(a => a.Project)
            .Where(a => a.EmployeeID == employeeId && a.Status != "Cancelled")
            .ToListAsync();

        var totalAllocated = activeAllocations.Sum(a => a.AllocationPercent);
        var available = Math.Max(0m, 100.00m - totalAllocated);

        var overlappingDtos = activeAllocations.Select(a => MapToResponseDto(a, a.Project?.Name ?? string.Empty, $"{employee.FirstName} {employee.LastName}")).ToList();

        return new AllocationConflictDto
        {
            EmployeeId = employeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            WeeklyCapacityHours = employee.WeeklyCapacityHours,
            TotalAllocatedPercent = totalAllocated,
            AvailablePercent = available,
            OverlappingAllocations = overlappingDtos
        };
    }

    public async Task<AllocationResponseDto> LockAllocationAsync(int allocationId)
    {
        var allocation = await _db.Allocations
            .Include(a => a.Project)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.AllocationID == allocationId);

        if (allocation is null)
        {
            throw new KeyNotFoundException($"Allocation with ID {allocationId} was not found.");
        }

        if (allocation.Status == "Cancelled")
        {
            throw new InvalidOperationException($"Cannot lock a cancelled allocation.");
        }

        if (allocation.Status == "Locked")
        {
            return MapToResponseDto(allocation, allocation.Project?.Name ?? string.Empty, $"{allocation.Employee?.FirstName} {allocation.Employee?.LastName}");
        }

        // Re-validate capacity before locking
        await ValidateEmployeeCapacityAsync(allocation.EmployeeID, allocation.AllocationID, allocation.AllocationPercent, allocation.StartDate, allocation.EndDate);

        allocation.Status = "Locked";
        await _db.SaveChangesAsync();

        return MapToResponseDto(allocation, allocation.Project?.Name ?? string.Empty, $"{allocation.Employee?.FirstName} {allocation.Employee?.LastName}");
    }

    public async Task<bool> DeleteAllocationAsync(int allocationId)
    {
        var allocation = await _db.Allocations.FindAsync(allocationId);
        if (allocation is null)
        {
            return false;
        }

        // Locking Enforcement: Prevent modification/deletion once locked
        if (allocation.Status == "Locked")
        {
            throw new InvalidOperationException($"Cannot delete or modify allocation {allocationId} because it is locked.");
        }

        _db.Allocations.Remove(allocation);
        await _db.SaveChangesAsync();

        return true;
    }

    private async Task ValidateEmployeeCapacityAsync(int employeeId, int? currentAllocationId, decimal requestedPercent, DateTime startDate, DateTime? endDate)
    {
        var endDateLimit = endDate ?? DateTime.MaxValue;

        var existingAllocations = await _db.Allocations
            .Where(a => a.EmployeeID == employeeId &&
                        a.Status != "Cancelled" &&
                        (!currentAllocationId.HasValue || a.AllocationID != currentAllocationId.Value))
            .ToListAsync();

        // Check date range overlap
        var overlappingAllocations = existingAllocations
            .Where(a => a.StartDate <= endDateLimit && (a.EndDate == null || a.EndDate.Value >= startDate))
            .ToList();

        var currentTotalPercent = overlappingAllocations.Sum(a => a.AllocationPercent);
        var projectCapacityTotal = currentTotalPercent + requestedPercent;

        if (projectCapacityTotal > 100.00m)
        {
            throw new InvalidOperationException($"Allocation rejected. Total allocation capacity for Employee {employeeId} would be {projectCapacityTotal}%, exceeding maximum 100% limit (Current overlapping: {currentTotalPercent}%, Requested: {requestedPercent}%).");
        }
    }

    private static AllocationResponseDto MapToResponseDto(Allocation a, string projectName, string employeeName)
    {
        return new AllocationResponseDto
        {
            AllocationId = a.AllocationID,
            ProjectId = a.ProjectID,
            ProjectName = projectName,
            EmployeeId = a.EmployeeID,
            EmployeeName = employeeName,
            ProjectRequirementId = a.ProjectRequirementID,
            AllocationPercent = a.AllocationPercent,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            Status = a.Status,
            CreatedBy = a.CreatedBy,
            CreatedDate = a.CreatedDate
        };
    }
}
