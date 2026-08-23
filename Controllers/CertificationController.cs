using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSync.Data;
using SkillSync.Models;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CertificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCertifications()
    {
        var certifications = await _context.Certifications
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.IssuingOrganization,
                c.Description
            })
            .ToListAsync();

        return Ok(certifications);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCertification(int id)
    {
        var certification = await _context.Certifications
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.IssuingOrganization,
                c.Description
            })
            .FirstOrDefaultAsync();

        return certification is null
            ? NotFound()
            : Ok(certification);
    }

    [HttpPost]
    [Authorize(Roles = "HR Administrator,System Administrator")]
    public async Task<IActionResult> CreateCertification(
        Certification certification)
    {
        _context.Certifications.Add(certification);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCertification),
            new { id = certification.Id },
            certification);
    }
}
