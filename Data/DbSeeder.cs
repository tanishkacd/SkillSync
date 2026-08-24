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
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var testUsers = new[]
{
    new { Email = "employee@skillsync.com", Role = "Employee" },
    new { Email = "pm@skillsync.com", Role = "Project Manager" },
    new { Email = "resource@skillsync.com", Role = "Resource Manager" },
    new { Email = "hr@skillsync.com", Role = "HR Administrator" },
    new { Email = "finance@skillsync.com", Role = "Finance / Operations" },
    new { Email = "admin@skillsync.com", Role = "System Administrator" }
};

        foreach (var testUser in testUsers)
        {
            var user = await userManager.FindByEmailAsync(testUser.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = testUser.Email,
                    Email = testUser.Email,
                    EmailConfirmed = true,
                    FullName = testUser.Role
                };

                var result = await userManager.CreateAsync(user, "SkillSync@123");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create {testUser.Email}: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, testUser.Role))
            {
                await userManager.AddToRoleAsync(user, testUser.Role);
            }
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
