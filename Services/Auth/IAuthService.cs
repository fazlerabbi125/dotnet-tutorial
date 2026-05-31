using Microsoft.AspNetCore.Identity;
using TutorialProj.Dtos.Auth;

namespace TutorialProj.Services.Auth;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterDto dto);
    Task<AuthTokenDto?> LoginAsync(LoginDto dto);
}
