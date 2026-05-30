using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace TutorialProj.Common;

/// <summary>
/// Argon2id password hasher for ASP.NET Core Identity (replaces the default PBKDF2-HMAC-SHA256).
/// OWASP-recommended parameters: m=19 MiB, t=2, p=1. Stored format: "{base64 salt}.{base64 hash}".
/// </summary>
public sealed class Argon2idPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
{
    private const int MemoryKib = 20 * 1024; // 20 MiB (slightly above OWASP's 19 MiB floor)
    private const int Iterations = 2;
    private const int Parallelism = 1;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string HashPassword(TUser user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, salt);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2) return PasswordVerificationResult.Failed;

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);
            var actual = ComputeHash(providedPassword, salt);
            return CryptographicOperations.FixedTimeEquals(actual, expected)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemoryKib,
            Iterations = Iterations,
        };
        return argon2.GetBytes(HashSize);
    }
}
