using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Models;

namespace SkillRadarReports.Data;

// NOTE: This DbContext currently only knows about the tables Person 7 owns
// (Milestones, Notifications). When Person 1+2 share the real schema/DbContext,
// either merge this into their AppDbContext, or add DbSets for Project/Employee
// here as read-only references so EF can join across them.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Milestone>()
            .Property(m => m.Status)
            .HasConversion<string>(); // store enum as readable text in DB

        modelBuilder.Entity<Notification>()
            .Property(n => n.Type)
            .HasConversion<string>();
    }
}
