using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TutorialProj.Constants;
using TutorialProj.Models;

namespace TutorialProj.Commands;

/// <summary>
/// Command for creating an admin user with user input validation.
/// </summary>
public class CreateAdminUserCommand
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<CreateAdminUserCommand> _logger;

    public CreateAdminUserCommand(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<CreateAdminUserCommand> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Creates an admin user interactively by prompting for email and password.
    /// </summary>
    public async Task ExecuteAsync()
    {
        Console.WriteLine("\n=== Create Admin User ===\n");

        var email = GetValidatedEmail();

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            Console.WriteLine($"User with email '{email}' already exists.");
            return;
        }

        var password = GetValidatedPassword();

        if (!await _roleManager.RoleExistsAsync(AppRoles.Manager))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(AppRoles.Manager));
            if (!roleResult.Succeeded)
            {
                PrintIdentityErrors("Failed to create Manager role", roleResult);
                return;
            }
        }

        var adminUser = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(adminUser, password);

        if (!result.Succeeded)
        {
            PrintIdentityErrors("Failed to create admin user", result);
            return;
        }

        var addRoleResult = await _userManager.AddToRoleAsync(adminUser, AppRoles.Manager);

        if (!addRoleResult.Succeeded)
        {
            PrintIdentityErrors("Failed to assign Manager role", addRoleResult);
            return;
        }

        Console.WriteLine("Admin user created successfully!");
        Console.WriteLine($"   Email: {email}");
        Console.WriteLine($"   Role: {AppRoles.Manager}\n");
        _logger.LogInformation("Admin user created: {Email}", email);
    }

    private static string GetValidatedEmail()
    {
        while (true)
        {
            Console.Write("Enter email address: ");
            var email = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email cannot be empty. Please try again.");
                continue;
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                Console.WriteLine("Invalid email format. Please try again.");
                continue;
            }

            if (email.Length > 256)
            {
                Console.WriteLine("Email is too long (max 256 characters). Please try again.");
                continue;
            }

            return email;
        }
    }

    private static string GetValidatedPassword()
    {
        while (true)
        {
            Console.Write("Enter password (min 6 characters): ");
            var password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password cannot be empty. Please try again.");
                continue;
            }

            if (password.Length < 6)
            {
                Console.WriteLine("Password must be at least 6 characters. Please try again.");
                continue;
            }

            if (password.Length > 128)
            {
                Console.WriteLine("Password is too long (max 128 characters). Please try again.");
                continue;
            }

            Console.Write("Confirm password: ");
            var confirmPassword = Console.ReadLine();

            if (password != confirmPassword)
            {
                Console.WriteLine("Passwords do not match. Please try again.");
                continue;
            }

            return password;
        }
    }

    private void PrintIdentityErrors(string message, IdentityResult result)
    {
        Console.WriteLine(message + ":");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"  - {error.Description}");
        }

        _logger.LogError("{Message}: {Errors}", message,
            string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
