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
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<EmployeeCertification> EmployeeCertifications => Set<EmployeeCertification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SkillCategory>()
            .HasMany(x => x.Skills)
            .WithOne(x => x.SkillCategory)
            .HasForeignKey(x => x.SkillCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EmployeeSkill>()
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmployeeSkill>()
            .Property(e => e.Score)
            .HasPrecision(5, 2);    

        builder.Entity<EmployeeSkill>()
            .HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EmployeeCertification>()
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmployeeCertification>()
            .HasOne(x => x.Certification)
            .WithMany()
            .HasForeignKey(x => x.CertificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EmployeeSkill>()
            .HasCheckConstraint("CK_EmployeeSkill_Score", "[Score] >= 0 AND [Score] <= 5");
    }
}
