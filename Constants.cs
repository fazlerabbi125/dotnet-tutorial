namespace TutorialProj.Constants;

/// <summary>
/// Shared application constants used across authentication, authorization, and seeding.
/// </summary>
public static class AppRoles
{
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] All = [Manager, User];
}

public static class AppCommands
{
    public const string CreateAdmin = "create-admin";
}
