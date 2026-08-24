using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;
using SkillRadarReports.Services;
using Xunit;

namespace SkillSync.Tests;

public class AllocationServiceTests
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

        if (!await db.Projects.AnyAsync(p => p.ProjectID == 1))
        {
            db.Projects.Add(new Project { ProjectID = 1, Name = "Project Alpha", ProjectManagerID = 100, StartDate = DateTime.UtcNow });
        }

        if (!await db.Projects.AnyAsync(p => p.ProjectID == 2))
        {
            db.Projects.Add(new Project { ProjectID = 2, Name = "Project Beta", ProjectManagerID = 100, StartDate = DateTime.UtcNow });
        }

        if (!await db.Projects.AnyAsync(p => p.ProjectID == 3))
        {
            db.Projects.Add(new Project { ProjectID = 3, Name = "Project Gamma", ProjectManagerID = 100, StartDate = DateTime.UtcNow });
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAllocationAsync_CreatesValidAllocation_WhenCapacityUnder100Percent()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(CreateAllocationAsync_CreatesValidAllocation_WhenCapacityUnder100Percent));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var dto = new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 60.00m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(3),
            Status = "Proposed",
            CreatedBy = 100
        };

        // Act
        var result = await service.CreateAllocationAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal(100, result.EmployeeId);
        Assert.Equal(60.00m, result.AllocationPercent);
        Assert.Equal("Proposed", result.Status);
    }

    [Fact]
    public async Task CreateAllocationAsync_RejectsAllocation_WhenCapacityExceeds100Percent()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(CreateAllocationAsync_RejectsAllocation_WhenCapacityExceeds100Percent));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);

        // Allocation 1: Project 1 = 60%
        await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 60.00m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(6),
            Status = "Proposed",
            CreatedBy = 100
        });

        // Allocation 2: Project 2 = 30% (Total = 90%)
        await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 2,
            EmployeeId = 100,
            AllocationPercent = 30.00m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(6),
            Status = "Proposed",
            CreatedBy = 100
        });

        // Allocation 3: Project 3 = 20% (Total = 110% -> must be rejected with 409 Conflict!)
        var dtoExceeding = new CreateAllocationDto
        {
            ProjectId = 3,
            EmployeeId = 100,
            AllocationPercent = 20.00m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(6),
            Status = "Proposed",
            CreatedBy = 100
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAllocationAsync(dtoExceeding));
        Assert.Contains("exceeding maximum 100% limit", ex.Message);
    }

    [Fact]
    public async Task CreateAllocationAsync_RejectsDuplicateAllocation_WithConflictException()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(CreateAllocationAsync_RejectsDuplicateAllocation_WithConflictException));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var startDate = DateTime.UtcNow.Date;

        // First creation
        await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 40.00m,
            StartDate = startDate,
            Status = "Proposed",
            CreatedBy = 100
        });

        // Duplicate POST call for same Project, Employee, StartDate
        var duplicateDto = new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 50.00m,
            StartDate = startDate,
            Status = "Proposed",
            CreatedBy = 100
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAllocationAsync(duplicateDto));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateAllocationAsync_UpdatesProposedAllocation_Successfully()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(UpdateAllocationAsync_UpdatesProposedAllocation_Successfully));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var created = await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 40.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Proposed",
            CreatedBy = 100
        });

        var updateDto = new UpdateAllocationDto
        {
            AllocationPercent = 70.00m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(3),
            Status = "Proposed"
        };

        // Act
        var updated = await service.UpdateAllocationAsync(created.AllocationId, updateDto);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(70.00m, updated.AllocationPercent);
    }

    [Fact]
    public async Task UpdateAllocationAsync_RejectsModification_WhenAllocationIsLocked()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(UpdateAllocationAsync_RejectsModification_WhenAllocationIsLocked));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var created = await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 40.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Proposed",
            CreatedBy = 100
        });

        await service.LockAllocationAsync(created.AllocationId);

        var updateDto = new UpdateAllocationDto
        {
            AllocationPercent = 50.00m,
            StartDate = DateTime.UtcNow.Date
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAllocationAsync(created.AllocationId, updateDto));
        Assert.Contains("locked", ex.Message.ToLower());
    }

    [Fact]
    public async Task GetEmployeeConflictsAsync_DetectsExistingOverlappingAllocations()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(GetEmployeeConflictsAsync_DetectsExistingOverlappingAllocations));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 70.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Locked",
            CreatedBy = 100
        });

        // Act
        var conflicts = await service.GetEmployeeConflictsAsync(100);

        // Assert
        Assert.NotNull(conflicts);
        Assert.Equal(100, conflicts.EmployeeId);
        Assert.Equal(70.00m, conflicts.TotalAllocatedPercent);
        Assert.Equal(30.00m, conflicts.AvailablePercent);
        Assert.Single(conflicts.OverlappingAllocations);
    }

    [Fact]
    public async Task LockAllocationAsync_LocksAllocationSuccessfully()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(LockAllocationAsync_LocksAllocationSuccessfully));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var created = await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 50.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Proposed",
            CreatedBy = 100
        });

        // Act
        var locked = await service.LockAllocationAsync(created.AllocationId);

        // Assert
        Assert.Equal("Locked", locked.Status);
    }

    [Fact]
    public async Task DeleteAllocationAsync_PreventsDeletion_WhenAllocationIsLocked()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(DeleteAllocationAsync_PreventsDeletion_WhenAllocationIsLocked));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var created = await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 50.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Proposed",
            CreatedBy = 100
        });

        await service.LockAllocationAsync(created.AllocationId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAllocationAsync(created.AllocationId));
        Assert.Contains("locked", ex.Message.ToLower());
    }

    [Fact]
    public async Task DeleteAllocationAsync_AllowsDeletion_WhenAllocationIsProposed()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(DeleteAllocationAsync_AllowsDeletion_WhenAllocationIsProposed));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var created = await service.CreateAllocationAsync(new CreateAllocationDto
        {
            ProjectId = 1,
            EmployeeId = 100,
            AllocationPercent = 50.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Proposed",
            CreatedBy = 100
        });

        // Act
        var deleted = await service.DeleteAllocationAsync(created.AllocationId);

        // Assert
        Assert.True(deleted);
        var exists = await db.Allocations.AnyAsync(a => a.AllocationID == created.AllocationId);
        Assert.False(exists);
    }

    [Fact]
    public async Task CreateAllocationAsync_ThrowsKeyNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(CreateAllocationAsync_ThrowsKeyNotFoundException_WhenProjectDoesNotExist));
        await SeedBaseDataAsync(db);

        var service = new AllocationService(db);
        var dto = new CreateAllocationDto
        {
            ProjectId = 999, // Non-existent project
            EmployeeId = 100,
            AllocationPercent = 50.00m,
            StartDate = DateTime.UtcNow.Date,
            Status = "Proposed",
            CreatedBy = 100
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAllocationAsync(dto));
    }
}
