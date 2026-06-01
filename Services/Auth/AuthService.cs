using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TutorialProj.Dtos.Auth;
using TutorialProj.Exceptions;
using TutorialProj.Models;
using TutorialProj.Common;

namespace TutorialProj.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppConfig _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppConfig config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            // All registered users get the User role. Admin users must be created via CreateAdminUserCommand.
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }

        return result;
    }

    public async Task<AuthTokenDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            throw new ApiException(
                "Account locked due to too many failed attempts. Try again later.",
                StatusCodes.Status423Locked);
        }
        if (!result.Succeeded) return null;

        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_config.JwtExpiryInMinutes),
            signingCredentials: creds
        );

        return new AuthTokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
