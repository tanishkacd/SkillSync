using System.ComponentModel.DataAnnotations;

namespace SkillRadarReports.Dtos;

public class CreateAllocationDto
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public int? ProjectRequirementId { get; set; }

    [Range(0.01, 100.00, ErrorMessage = "AllocationPercent must be between 0.01 and 100.00.")]
    public decimal AllocationPercent { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = "Proposed"; // Proposed or Locked

    [Required]
    public int CreatedBy { get; set; }
}

public class UpdateAllocationStatusDto
{
    [Required]
    public string Status { get; set; } = "Locked"; // Proposed, Locked, Completed, Cancelled
}

public class UpdateAllocationDto
{
    [Range(0.01, 100.00, ErrorMessage = "AllocationPercent must be between 0.01 and 100.00.")]
    public decimal AllocationPercent { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }
}

public class AllocationResponseDto
{
    public int AllocationId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? ProjectRequirementId { get; set; }
    public decimal AllocationPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AllocationConflictDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal WeeklyCapacityHours { get; set; }
    public decimal TotalAllocatedPercent { get; set; }
    public decimal AvailablePercent { get; set; }
    public List<AllocationResponseDto> OverlappingAllocations { get; set; } = new();
}
