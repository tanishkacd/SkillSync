using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSync.Models;

namespace SkillSync.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<EmployeeCertification> EmployeeCertifications => Set<EmployeeCertification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Department>()
       .ToTable("Department");

        builder.Entity<Employee>()
            .ToTable("Employee");

        builder.Entity<Employee>()
    .HasOne(e => e.Department)
    .WithMany(d => d.Employees)
    .HasForeignKey(e => e.DepartmentID)
    .OnDelete(DeleteBehavior.Restrict);

        // SkillCategory -> Skills
        builder.Entity<SkillCategory>()
            .HasMany(x => x.Skills)
            .WithOne(x => x.SkillCategory)
            .HasForeignKey(x => x.SkillCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee -> EmployeeSkills
        builder.Entity<EmployeeSkill>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeSkills)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Skill -> EmployeeSkills
        builder.Entity<EmployeeSkill>()
            .HasOne(x => x.Skill)
            .WithMany(x => x.EmployeeSkills)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee -> EmployeeCertifications
        builder.Entity<EmployeeCertification>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeCertifications)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Certification -> EmployeeCertifications
        builder.Entity<EmployeeCertification>()
            .HasOne(x => x.Certification)
            .WithMany(x => x.EmployeeCertifications)
            .HasForeignKey(x => x.CertificationId)
            .OnDelete(DeleteBehavior.Restrict);

        // EmployeeSkill Score
        builder.Entity<EmployeeSkill>()
            .Property(x => x.Score)
            .HasPrecision(5, 2);

        builder.Entity<EmployeeSkill>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_EmployeeSkill_Score",
                "[Score] >= 0 AND [Score] <= 5"));
    }
}