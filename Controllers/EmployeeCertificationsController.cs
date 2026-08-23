using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSync.Data;
using SkillSync.Models;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeCertificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeeCertificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/EmployeeCertifications
    [HttpGet]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<IActionResult> GetEmployeeCertifications()
    {
        var certifications = await _context.EmployeeCertifications
            .Include(ec => ec.Employee)
            .Include(ec => ec.Certification)
            .Select(ec => new
            {
                ec.EmployeeCertificationId,
                ec.EmployeeId,
                ec.CertificationId,
                Certification = ec.Certification.Name,
                ec.IssueDate,
                ec.ExpiryDate
            })
            .ToListAsync();

        return Ok(certifications);
    }

    // GET: api/EmployeeCertifications/employee/5
    [HttpGet("employee/{employeeId:int}")]
    [Authorize(Roles = "Resource Manager,HR Administrator,System Administrator")]
    public async Task<IActionResult> GetEmployeeCertificationsByEmployee(
        int employeeId)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId);

        if (!employeeExists)
        {
            return NotFound(new
            {
                message = "Employee not found."
            });
        }

        var certifications = await _context.EmployeeCertifications
            .Where(ec => ec.EmployeeId == employeeId)
            .Include(ec => ec.Certification)
            .Select(ec => new
            {
                ec.EmployeeCertificationId,
                ec.EmployeeId,
                ec.CertificationId,
                Certification = ec.Certification.Name,
                ec.IssueDate,
                ec.ExpiryDate
            })
            .ToListAsync();

        return Ok(certifications);
    }

    // POST: api/EmployeeCertifications
    [HttpPost]
    [Authorize(Roles = "HR Administrator,System Administrator")]
    public async Task<IActionResult> AddEmployeeCertification(
        EmployeeCertification employeeCertification)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeCertification.EmployeeId);

        if (!employeeExists)
        {
            return BadRequest(new
            {
                message = "Employee does not exist."
            });
        }

        var certificationExists = await _context.Certifications
            .AnyAsync(c => c.Id == employeeCertification.CertificationId);

        if (!certificationExists)
        {
            return BadRequest(new
            {
                message = "Certification does not exist."
            });
        }

        var duplicate = await _context.EmployeeCertifications
            .AnyAsync(ec =>
                ec.EmployeeId == employeeCertification.EmployeeId &&
                ec.CertificationId == employeeCertification.CertificationId);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "Employee already has this certification."
            });
        }

        _context.EmployeeCertifications.Add(employeeCertification);
        await _context.SaveChangesAsync();

        return Ok(employeeCertification);
    }

    // DELETE: api/EmployeeCertifications/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "HR Administrator,System Administrator")]
    public async Task<IActionResult> DeleteEmployeeCertification(int id)
    {
        var employeeCertification =
            await _context.EmployeeCertifications.FindAsync(id);

        if (employeeCertification == null)
        {
            return NotFound(new
            {
                message = "Employee certification not found."
            });
        }

        _context.EmployeeCertifications.Remove(employeeCertification);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
