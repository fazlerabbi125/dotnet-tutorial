using Microsoft.AspNetCore.Mvc;
using DotNetTutorial.Models;
using DotNetTutorial.Repositories;
using DotNetTutorial.Validation;
using DotNetTutorial.Services;

namespace DotNetTutorial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly AuthService _authService;

        public AuthController(UserRepository userRepository, RefreshTokenRepository refreshTokenRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateSchema request)
        {
            var errors = DataValidator.ValidateSchema(request);
            if (errors.Count > 0)
                return BadRequest(errors);

            var existingUser = await _userRepository.GetByUsername(request.Username);
            if (existingUser != null)
                return Conflict(new { message = "Username already exists" });

            var newUser = new User
            {
                Username = DataValidator.Sanitize(request.Username),
                Email = DataValidator.Sanitize(request.Email),
                PasswordHash = _authService.HashPassword(request.Password)
            };

            await _userRepository.InsertUser(newUser);

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginSchema request)
        {
            var user = await _userRepository.GetByUsername(request.Username);

            if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password" });

            var accessToken = _authService.GenerateJwtToken(user);
            var rawRefreshToken = _authService.GenerateRefreshToken();

            // Store the new refresh token in its own table
            await _refreshTokenRepository.Create(user.UserID, rawRefreshToken, DateTime.UtcNow.AddDays(7));

            Response.Cookies.Append("refreshToken", rawRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { token = accessToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(rawRefreshToken))
                return Unauthorized(new { message = "Refresh token is missing" });

            // Look up the token in the RefreshTokens table
            var storedToken = await _refreshTokenRepository.GetByToken(rawRefreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            // Revoke old token (rotation)
            await _refreshTokenRepository.Revoke(rawRefreshToken);

            var user = await _userRepository.GetOne(storedToken.UserID);
            if (user == null) return Unauthorized(new { message = "User not found" });

            var newAccessToken = _authService.GenerateJwtToken(user);
            var newRawRefreshToken = _authService.GenerateRefreshToken();

            await _refreshTokenRepository.Create(user.UserID, newRawRefreshToken, DateTime.UtcNow.AddDays(7));

            Response.Cookies.Append("refreshToken", newRawRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { token = newAccessToken });
        }
    }
}
