using E_commerce.Data.Entities;
using E_commerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_commerce.Api.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseSeeder));
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            logger.LogInformation("Applying database migrations");
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed");
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Database migration failed during startup");
            throw;
        }
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (await roles.RoleExistsAsync(role)) continue;
            var result = await roles.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not seed role '{role}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
            logger.LogInformation("Seeded role {RoleName}", role);
        }
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var admin = await users.FindByNameAsync("admin");
        if (admin is null)
        {
            admin = new User { UserName = "admin", FullName = "Admin User", Email = "admin@ecommerce.local", EmailConfirmed = true };
            var created = await users.CreateAsync(admin, "123456");
            if (!created.Succeeded) throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
            logger.LogInformation("Seeded initial administrator account");
        }
        if (!await users.IsInRoleAsync(admin, "Admin"))
        {
            var result = await users.AddToRoleAsync(admin, "Admin");
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not assign the Admin role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            logger.LogInformation("Assigned the Admin role to the initial administrator");
        }
    }
}
