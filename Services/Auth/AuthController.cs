using Microsoft.AspNetCore.Mvc;
using TutorialProj.Dtos.Auth;

namespace TutorialProj.Services.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (result.Succeeded)
        {
            return Ok(new { message = "User registered successfully" });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var tokenResult = await _authService.LoginAsync(dto);

        if (tokenResult == null)
        {
            return Unauthorized(new { error = "Invalid email or password" });
        }

        return Ok(tokenResult);
    }
}
