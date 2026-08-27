namespace SkillRadarReports.Models;

public class Employee
{
    public int EmployeeID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
    public string? JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public decimal WeeklyCapacityHours { get; set; } = 40.00m;
    public decimal CostRatePerHour { get; set; }
    public bool IsActive { get; set; } = true;
    public string? IdentityUserId { get; set; }

    public Department? Department { get; set; }
}
