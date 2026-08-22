using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSync.Models;

namespace SkillSync.Data;

public static class DbSeeder
{
    private static readonly string[] Roles =
    {
        "Employee",
        "Project Manager",
        "Resource Manager",
        "HR Administrator",
        "Finance / Operations",
        "System Administrator"
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!await context.SkillCategories.AnyAsync())
        {
            var programming = new SkillCategory
            {
                Name = "Programming",
                Description = "Programming languages and development skills"
            };

            var cloud = new SkillCategory
            {
                Name = "Cloud",
                Description = "Cloud platforms and services"
            };

            var database = new SkillCategory
            {
                Name = "Database",
                Description = "Database technologies"
            };

            context.SkillCategories.AddRange(programming, cloud, database);
            await context.SaveChangesAsync();

            context.Skills.AddRange(
                new Skill { Name = "C#", SkillCategoryId = programming.Id },
                new Skill { Name = "Java", SkillCategoryId = programming.Id },
                new Skill { Name = "Python", SkillCategoryId = programming.Id },
                new Skill { Name = "AWS", SkillCategoryId = cloud.Id },
                new Skill { Name = "Azure", SkillCategoryId = cloud.Id },
                new Skill { Name = "SQL Server", SkillCategoryId = database.Id },
                new Skill { Name = "MySQL", SkillCategoryId = database.Id }
            );

            await context.SaveChangesAsync();
        }
    }
}
