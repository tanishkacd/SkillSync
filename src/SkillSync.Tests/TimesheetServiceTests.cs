using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;
using SkillRadarReports.Services;
using Xunit;

namespace SkillSync.Tests;

public class TimesheetServiceTests
{
    private static AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedBaseDataAsync(AppDbContext db)
    {
        if (!await db.Departments.AnyAsync(d => d.DepartmentID == 1))
        {
            db.Departments.Add(new Department { DepartmentID = 1, Name = "Engineering" });
        }

        if (!await db.Employees.AnyAsync(e => e.EmployeeID == 100))
        {
            db.Employees.Add(new Employee
            {
                EmployeeID = 100,
                FirstName = "Rahul",
                LastName = "Sharma",
                Email = "rahul@skillsync.com",
                DepartmentID = 1,
                HireDate = DateTime.UtcNow,
                WeeklyCapacityHours = 40.00m,
                CostRatePerHour = 50.00m
            });
        }

        if (!await db.Employees.AnyAsync(e => e.EmployeeID == 200))
        {
            db.Employees.Add(new Employee
            {
                EmployeeID = 200,
                FirstName = "Anita",
                LastName = "Deshmukh",
                Email = "anita@skillsync.com",
                DepartmentID = 1,
                HireDate = DateTime.UtcNow,
                WeeklyCapacityHours = 40.00m,
                CostRatePerHour = 65.00m
            });
        }

        if (!await db.Projects.AnyAsync(p => p.ProjectID == 1))
        {
            db.Projects.Add(new Project
            {
                ProjectID = 1,
                Name = "Project Alpha",
                ProjectManagerID = 200,
                StartDate = DateTime.UtcNow
            });
        }

        if (!await db.Milestones.AnyAsync(m => m.Id == 10))
        {
            db.Milestones.Add(new Milestone
            {
                Id = 10,
                ProjectId = 1,
                Title = "Phase 1 Delivery",
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = MilestoneStatus.InProgress
            });
        }

        if (!await db.Tasks.AnyAsync(t => t.TaskID == 50))
        {
            db.Tasks.Add(new TaskModel
            {
                TaskID = 50,
                MilestoneID = 10,
                Name = "Frontend UI Layout",
                Status = "In Progress"
            });
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetOrInitializeTimesheetAsync_CreatesDraftTimesheet_WhenNotExists()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(GetOrInitializeTimesheetAsync_CreatesDraftTimesheet_WhenNotExists));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var today = DateTime.UtcNow.Date;

        // Act
        var result = await service.GetOrInitializeTimesheetAsync(100, today);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.EmployeeID);
        Assert.Equal("Draft", result.Status);
        Assert.Equal(0.00m, result.TotalHours);
    }

    [Fact]
    public async Task SaveTimesheetAsync_SavesValidEntries_Successfully()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(SaveTimesheetAsync_SavesValidEntries_Successfully));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var monday = TimesheetService.GetMondayOfWeek(DateTime.UtcNow);

        var dto = new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = new List<SaveTimesheetEntryDto>
            {
                new SaveTimesheetEntryDto
                {
                    ProjectID = 1,
                    TaskID = 50,
                    EntryDate = monday,
                    HoursWorked = 8.00m,
                    Notes = "Coding feature X"
                },
                new SaveTimesheetEntryDto
                {
                    ProjectID = 1,
                    TaskID = 50,
                    EntryDate = monday.AddDays(1),
                    HoursWorked = 7.50m,
                    Notes = "Bug fixes"
                }
            }
        };

        // Act
        var result = await service.SaveTimesheetAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15.50m, result.TotalHours);
        Assert.Equal(2, result.Entries.Count);
    }

    [Fact]
    public async Task SaveTimesheetAsync_RejectsEntry_WhenHoursWorkedOutside0To24()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(SaveTimesheetAsync_RejectsEntry_WhenHoursWorkedOutside0To24));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var monday = TimesheetService.GetMondayOfWeek(DateTime.UtcNow);

        var dto = new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = new List<SaveTimesheetEntryDto>
            {
                new SaveTimesheetEntryDto
                {
                    ProjectID = 1,
                    EntryDate = monday,
                    HoursWorked = 25.00m // Invalid (> 24)
                }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.SaveTimesheetAsync(dto));
        Assert.Contains("between 0 and 24", ex.Message);
    }

    [Fact]
    public async Task SaveTimesheetAsync_RejectsWeeklyTotal_Exceeding168Hours()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(SaveTimesheetAsync_RejectsWeeklyTotal_Exceeding168Hours));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var monday = TimesheetService.GetMondayOfWeek(DateTime.UtcNow);

        // 7 days * 24 hours = 168 hours max. Let's send 8 entries of 22 hours = 176 hours total
        var entries = new List<SaveTimesheetEntryDto>();
        for (int i = 0; i < 8; i++)
        {
            entries.Add(new SaveTimesheetEntryDto
            {
                ProjectID = 1,
                EntryDate = monday.AddDays(i % 7),
                HoursWorked = 22.00m
            });
        }

        var dto = new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = entries
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.SaveTimesheetAsync(dto));
        Assert.Contains("cannot exceed 168 hours", ex.Message);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_TransitionsStatusToSubmitted_AndLocksEditing()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(SubmitTimesheetAsync_TransitionsStatusToSubmitted_AndLocksEditing));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var monday = TimesheetService.GetMondayOfWeek(DateTime.UtcNow);

        var saved = await service.SaveTimesheetAsync(new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = new List<SaveTimesheetEntryDto>
            {
                new SaveTimesheetEntryDto { ProjectID = 1, EntryDate = monday, HoursWorked = 8.00m }
            }
        });

        // Act
        var submitted = await service.SubmitTimesheetAsync(saved.TimesheetID);

        // Assert
        Assert.Equal("Submitted", submitted.Status);

        // Verify editing submitted timesheet throws InvalidOperationException
        var updateAttempt = new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = new List<SaveTimesheetEntryDto>
            {
                new SaveTimesheetEntryDto { ProjectID = 1, EntryDate = monday, HoursWorked = 4.00m }
            }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveTimesheetAsync(updateAttempt));
        Assert.Contains("cannot be modified", ex.Message);
    }

    [Fact]
    public async Task ApproveTimesheetAsync_TransitionsStatusToApproved_AndSetsApproverInfo()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(ApproveTimesheetAsync_TransitionsStatusToApproved_AndSetsApproverInfo));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var monday = TimesheetService.GetMondayOfWeek(DateTime.UtcNow);

        var saved = await service.SaveTimesheetAsync(new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = new List<SaveTimesheetEntryDto>
            {
                new SaveTimesheetEntryDto { ProjectID = 1, EntryDate = monday, HoursWorked = 8.00m }
            }
        });

        await service.SubmitTimesheetAsync(saved.TimesheetID);

        // Act
        var approved = await service.ApproveTimesheetAsync(saved.TimesheetID, 200); // 200 = Manager Anita Deshmukh

        // Assert
        Assert.Equal("Approved", approved.Status);
        Assert.Equal(200, approved.ApprovedBy);
        Assert.Equal("Anita Deshmukh", approved.ApprovedByName);
        Assert.NotNull(approved.ApprovedDate);
    }

    [Fact]
    public async Task RejectTimesheetAsync_TransitionsStatusToRejected()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(RejectTimesheetAsync_TransitionsStatusToRejected));
        await SeedBaseDataAsync(db);
        var service = new TimesheetService(db);
        var monday = TimesheetService.GetMondayOfWeek(DateTime.UtcNow);

        var saved = await service.SaveTimesheetAsync(new SaveTimesheetDto
        {
            EmployeeID = 100,
            WeekStartDate = monday,
            Entries = new List<SaveTimesheetEntryDto>
            {
                new SaveTimesheetEntryDto { ProjectID = 1, EntryDate = monday, HoursWorked = 8.00m }
            }
        });

        await service.SubmitTimesheetAsync(saved.TimesheetID);

        // Act
        var rejected = await service.RejectTimesheetAsync(saved.TimesheetID, 200, "Incorrect project logged");

        // Assert
        Assert.Equal("Rejected", rejected.Status);
    }
}
