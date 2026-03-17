using NUnit.Framework;
using DotNetTutorial.Services;
using DotNetTutorial.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace DotNetTutorial.Tests
{
    [TestFixture]
    public class TestAuthService
    {
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            // Ensure JWT_SECRET is set for tests
            Environment.SetEnvironmentVariable("JWT_SECRET", "test_secret_key_needs_to_be_at_least_32_characters_long_for_hmac_sha256");
            _authService = new AuthService();
        }

        [Test]
        public void TestPasswordHashing()
        {
            string password = "StrongPassword123!";
            
            // Hash password
            string hash = _authService.HashPassword(password);
            
            Assert.IsNotEmpty(hash, "Password hash should not be empty");
            Assert.AreNotEqual(password, hash, "Password hash should not equal the plain text password");

            // Verify password
            bool isMatch = _authService.VerifyPassword(password, hash);
            Assert.IsTrue(isMatch, "Verification should return true for correct password");

            // Verify wrong password
            bool isWrongMatch = _authService.VerifyPassword("WrongPassword123!", hash);
            Assert.IsFalse(isWrongMatch, "Verification should return false for incorrect password");
        }

        [Test]
        public void TestJwtTokenGeneration()
        {
            var user = new User
            {
                UserID = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = "Admin"
            };

            string token = _authService.GenerateJwtToken(user);
            Assert.IsNotEmpty(token, "Generated JWT token should not be empty");

            // Optionally decode and check claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("testuser", jwtToken.Claims.First(c => c.Type == "unique_name").Value); // unique_name maps to ClaimTypes.Name in JwtSecurityTokenHandler by default
            Assert.AreEqual("Admin", jwtToken.Claims.First(c => c.Type == "role").Value); // role maps to ClaimTypes.Role
        }
        
        [Test]
        public void TestRefreshTokenGeneration()
        {
            string refreshToken1 = _authService.GenerateRefreshToken();
            string refreshToken2 = _authService.GenerateRefreshToken();
            
            Assert.IsNotEmpty(refreshToken1, "Refresh token should not be empty");
            Assert.AreNotEqual(refreshToken1, refreshToken2, "Generated refresh tokens should be unique");
        }
    }
}
