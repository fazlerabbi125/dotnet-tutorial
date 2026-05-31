using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TutorialProj.Models;

namespace TutorialProj.Commands;

/// <summary>
/// Command for creating an admin user with user input validation.
/// </summary>
public class CreateAdminUserCommand
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateAdminUserCommand> _logger;

    public CreateAdminUserCommand(
        UserManager<ApplicationUser> userManager,
        ILogger<CreateAdminUserCommand> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Creates an admin user interactively by prompting for email and password.
    /// </summary>
    public async Task ExecuteAsync()
    {
        Console.WriteLine("\n=== Create Admin User ===\n");

        // Get and validate email
        string email = GetValidatedEmail();

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            Console.WriteLine($"❌ User with email '{email}' already exists.");
            return;
        }

        // Get and validate password
        string password = GetValidatedPassword();

        // Create the user
        var adminUser = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(adminUser, password);

        if (!result.Succeeded)
        {
            Console.WriteLine("❌ Failed to create user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
            _logger.LogError("Failed to create admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        // Assign Manager role
        var roleResult = await _userManager.AddToRoleAsync(adminUser, "Manager");

        if (!roleResult.Succeeded)
        {
            Console.WriteLine("❌ Failed to assign Manager role:");
            foreach (var error in roleResult.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
            _logger.LogError("Failed to assign Manager role to admin user: {Errors}",
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            return;
        }

        Console.WriteLine($"✅ Admin user created successfully!");
        Console.WriteLine($"   Email: {email}");
        Console.WriteLine($"   Role: Manager\n");
        _logger.LogInformation("Admin user created: {Email}", email);
    }

    /// <summary>
    /// Prompts user for email and validates it.
    /// </summary>
    private static string GetValidatedEmail()
    {
        while (true)
        {
            Console.Write("Enter email address: ");
            string? email = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("❌ Email cannot be empty. Please try again.");
                continue;
            }

            // Validate email format
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                Console.WriteLine("❌ Invalid email format. Please try again.");
                continue;
            }

            if (email.Length > 256)
            {
                Console.WriteLine("❌ Email is too long (max 256 characters). Please try again.");
                continue;
            }

            return email;
        }
    }

    /// <summary>
    /// Prompts user for password and validates it.
    /// </summary>
    private static string GetValidatedPassword()
    {
        while (true)
        {
            Console.Write("Enter password (min 6 characters): ");
            string? password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("❌ Password cannot be empty. Please try again.");
                continue;
            }

            if (password.Length < 6)
            {
                Console.WriteLine("❌ Password must be at least 6 characters. Please try again.");
                continue;
            }

            if (password.Length > 128)
            {
                Console.WriteLine("❌ Password is too long (max 128 characters). Please try again.");
                continue;
            }

            // Confirm password
            Console.Write("Confirm password: ");
            string? confirmPassword = Console.ReadLine();

            if (password != confirmPassword)
            {
                Console.WriteLine("❌ Passwords do not match. Please try again.");
                continue;
            }

            return password;
        }
    }
}
