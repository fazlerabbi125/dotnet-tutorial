using Microsoft.AspNetCore.Identity;

namespace TutorialProj.Models;

/// <summary>
/// Custom application user extending IdentityUser.
/// You can add custom properties here (e.g., FirstName, LastName) that will become columns in the AspNetUsers table.
/// </summary>
public class ApplicationUser : IdentityUser
{
}
