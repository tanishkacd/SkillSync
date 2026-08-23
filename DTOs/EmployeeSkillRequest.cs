namespace SkillSync.DTOs;

public class EmployeeSkillRequest
{
    public int EmployeeId { get; set; }

    public int SkillId { get; set; }

    public decimal Score { get; set; }
}
