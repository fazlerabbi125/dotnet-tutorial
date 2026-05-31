using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TutorialProj.Constants;
using TutorialProj.Models;

namespace TutorialProj.Data;

/// <summary>
/// Seeds initial data into the database. Called automatically at startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, bool seedAdminFromEnvironment = true)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // 1. Apply any pending EF Core migrations automatically
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed");
            throw;
        }

        try
        {
            // 2. Add test inventory item if none exist
            if (!await context.InventoryItems.AnyAsync())
            {
                var item = new InventoryItem
                {
                    Name = "Pallet Jack",
                    Quantity = 12,
                    Location = "Warehouse A"
                };

                await context.InventoryItems.AddAsync(item);
                await context.SaveChangesAsync();

                logger.LogInformation("Test inventory item created");
                item.DisplayInfo();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed inventory items");
            throw;
        }

        try
        {
            // 3. Seed Roles for Identity
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    logger.LogInformation("Role '{Role}' created", roleName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed roles");
            throw;
        }

        if (!seedAdminFromEnvironment)
        {
            return;
        }

        try
        {
            // 4. Seed default admin user from environment variables, when provided.
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<AppConfig>();

            if (!await userManager.Users.AnyAsync())
            {
                var adminEmail = config.AdminEmail;
                var adminPassword = config.AdminPassword;

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                {
                    logger.LogWarning(
                        "Admin credentials are not configured. Run dotnet run -- create-admin to create one interactively.");
                }
                else
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, AppRoles.Manager);
                        logger.LogInformation("Default admin user created: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create default admin user. Errors: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed admin user");
            throw;
        }
    }
}
