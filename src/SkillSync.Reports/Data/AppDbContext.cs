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
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectRequirement> ProjectRequirements => Set<ProjectRequirement>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<TaskModel> Tasks => Set<TaskModel>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Milestone>()
            .Property(m => m.Status)
            .HasConversion<string>(); // store enum as readable text in DB

        modelBuilder.Entity<Notification>()
            .Property(n => n.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");
            entity.HasKey(e => e.DepartmentID);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skill");
            entity.HasKey(e => e.SkillID);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");
            entity.HasKey(e => e.EmployeeID);
            entity.Property(e => e.WeeklyCapacityHours).HasPrecision(5, 2);
            entity.Property(e => e.CostRatePerHour).HasPrecision(10, 2);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentID);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project");
            entity.HasKey(e => e.ProjectID);
            entity.Property(e => e.BudgetAmount).HasPrecision(14, 2);
            entity.Property(e => e.BillingRatePerHour).HasPrecision(10, 2);
            entity.HasOne(e => e.ProjectManager)
                .WithMany()
                .HasForeignKey(e => e.ProjectManagerID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectRequirement>(entity =>
        {
            entity.ToTable("ProjectRequirement");
            entity.HasKey(e => e.ProjectRequirementID);
            entity.Property(e => e.MinYearsExperience).HasPrecision(4, 1);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Requirements)
                .HasForeignKey(e => e.ProjectID);
            entity.HasOne(e => e.Skill)
                .WithMany()
                .HasForeignKey(e => e.SkillID);
        });

        modelBuilder.Entity<Allocation>(entity =>
        {
            entity.ToTable("Allocation");
            entity.HasKey(e => e.AllocationID);
            entity.Property(e => e.AllocationPercent).HasPrecision(5, 2);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Allocations)
                .HasForeignKey(e => e.ProjectID);
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeID);
            entity.HasOne(e => e.ProjectRequirement)
                .WithMany()
                .HasForeignKey(e => e.ProjectRequirementID)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskModel>(entity =>
        {
            entity.ToTable("Task");
            entity.HasKey(e => e.TaskID);
            entity.HasOne(e => e.Milestone)
                .WithMany()
                .HasForeignKey(e => e.MilestoneID);
        });

        modelBuilder.Entity<Timesheet>(entity =>
        {
            entity.ToTable("Timesheet");
            entity.HasKey(e => e.TimesheetID);
            entity.HasIndex(e => new { e.EmployeeID, e.WeekStartDate }).IsUnique();
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeID);
            entity.HasOne(e => e.Approver)
                .WithMany()
                .HasForeignKey(e => e.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TimesheetEntry>(entity =>
        {
            entity.ToTable("TimesheetEntry");
            entity.HasKey(e => e.TimesheetEntryID);
            entity.Property(e => e.HoursWorked).HasPrecision(4, 2);
            entity.HasOne(e => e.Timesheet)
                .WithMany(t => t.Entries)
                .HasForeignKey(e => e.TimesheetID);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectID);
            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskID)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
