using Microsoft.EntityFrameworkCore;
using TutorialProj.Models;

namespace TutorialProj.Data;

/// <summary>
/// Seeds initial data into the database. Called automatically at startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Apply any pending EF Core migrations automatically
        await context.Database.MigrateAsync();

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
            
            // Console output to confirm it worked as requested in Step 5
            item.DisplayInfo();
        }

        // 3. Seed Roles and Admin user for Identity
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

        string[] roleNames = { "Manager", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(roleName));
            }
        }

        var adminEmail = "admin@logitrack.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Manager");
            }
        }
    }
}
