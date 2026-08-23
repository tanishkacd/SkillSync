using System.Collections.Generic;

namespace SkillSync.Models;

public class Certification
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? IssuingOrganization { get; set; }

    public string? Description { get; set; }

    public ICollection<EmployeeCertification> EmployeeCertifications { get; set; }
        = new List<EmployeeCertification>();
}