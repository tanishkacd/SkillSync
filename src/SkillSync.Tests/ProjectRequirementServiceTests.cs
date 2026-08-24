using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;
using SkillRadarReports.Services;
using Xunit;

namespace SkillSync.Tests;

public class ProjectRequirementServiceTests
{
    private static AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddRequirementAsync_CreatesValidRequirement()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(AddRequirementAsync_CreatesValidRequirement));

        db.Projects.Add(new Project { ProjectID = 1, Name = "Alpha Project", ProjectManagerID = 10, StartDate = DateTime.UtcNow });
        db.Skills.Add(new Skill { SkillID = 5, SkillCategoryID = 1, Name = "C#" });
        await db.SaveChangesAsync();

        var service = new ProjectRequirementService(db);
        var dto = new CreateProjectRequirementDto
        {
            SkillId = 5,
            MinProficiency = 4,
            MinYearsExperience = 3.0m,
            HeadcountNeeded = 2
        };

        // Act
        var result = await service.AddRequirementAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal(5, result.SkillId);
        Assert.Equal("C#", result.SkillName);
        Assert.Equal(4, result.MinProficiency);
        Assert.Equal(3.0m, result.MinYearsExperience);
        Assert.Equal(2, result.HeadcountNeeded);
    }

    [Fact]
    public async Task AddRequirementAsync_RejectsDuplicateEntryWithConflictException()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(AddRequirementAsync_RejectsDuplicateEntryWithConflictException));

        db.Projects.Add(new Project { ProjectID = 1, Name = "Alpha Project", ProjectManagerID = 10, StartDate = DateTime.UtcNow });
        db.Skills.Add(new Skill { SkillID = 5, SkillCategoryID = 1, Name = "C#" });
        db.ProjectRequirements.Add(new ProjectRequirement { ProjectRequirementID = 100, ProjectID = 1, SkillID = 5, MinProficiency = 2, MinYearsExperience = 1.0m, HeadcountNeeded = 1 });
        await db.SaveChangesAsync();

        var service = new ProjectRequirementService(db);
        var dto = new CreateProjectRequirementDto
        {
            SkillId = 5,
            MinProficiency = 5,
            MinYearsExperience = 4.0m,
            HeadcountNeeded = 3
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddRequirementAsync(1, dto));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateRequirementAsync_UpdatesExistingRequirement()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(UpdateRequirementAsync_UpdatesExistingRequirement));

        db.Projects.Add(new Project { ProjectID = 1, Name = "Alpha Project", ProjectManagerID = 10, StartDate = DateTime.UtcNow });
        db.Skills.Add(new Skill { SkillID = 5, SkillCategoryID = 1, Name = "C#" });
        db.ProjectRequirements.Add(new ProjectRequirement { ProjectRequirementID = 100, ProjectID = 1, SkillID = 5, MinProficiency = 2, MinYearsExperience = 1.0m, HeadcountNeeded = 1 });
        await db.SaveChangesAsync();

        var service = new ProjectRequirementService(db);
        var dto = new UpdateProjectRequirementDto
        {
            MinProficiency = 5,
            MinYearsExperience = 4.0m,
            HeadcountNeeded = 3
        };

        // Act
        var result = await service.UpdateRequirementAsync(1, 100, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.MinProficiency);
        Assert.Equal(4.0m, result.MinYearsExperience);
        Assert.Equal(3, result.HeadcountNeeded);
    }

    [Fact]
    public async Task GetRequirementsByProjectIdAsync_ReturnsRequirements_WhenProjectExists()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(GetRequirementsByProjectIdAsync_ReturnsRequirements_WhenProjectExists));

        db.Projects.Add(new Project { ProjectID = 2, Name = "Beta Project", ProjectManagerID = 10, StartDate = DateTime.UtcNow });
        db.Skills.Add(new Skill { SkillID = 1, SkillCategoryID = 1, Name = "SQL Server" });
        db.ProjectRequirements.Add(new ProjectRequirement { ProjectRequirementID = 10, ProjectID = 2, SkillID = 1, MinProficiency = 3, MinYearsExperience = 2.0m, HeadcountNeeded = 1 });
        await db.SaveChangesAsync();

        var service = new ProjectRequirementService(db);

        // Act
        var results = await service.GetRequirementsByProjectIdAsync(2);

        // Assert
        Assert.Single(results);
        Assert.Equal("SQL Server", results[0].SkillName);
    }

    [Fact]
    public async Task GetRequirementsByProjectIdAsync_ThrowsKeyNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        using var db = GetInMemoryDbContext(nameof(GetRequirementsByProjectIdAsync_ThrowsKeyNotFoundException_WhenProjectDoesNotExist));
        var service = new ProjectRequirementService(db);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetRequirementsByProjectIdAsync(999));
    }
}
