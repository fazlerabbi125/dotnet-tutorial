using NUnit.Framework;
using DotNetTutorial.Validation;

namespace DotNetTutorial.Tests // Added namespace for good practice
{
    [TestFixture]
    public class TestInputValidation
    {
        // [Test]
        // public void TestSanitization()
        // {
        //     // Test XSS prevention: Tags should be stripped
        //     string xssPayload = "<script>alert('XSS')</script>";
        //     string sanitizedXSS = DataValidator.Sanitize(xssPayload);
        //     Assert.AreEqual("", sanitizedXSS, "XSS tags should be completely removed.");
        //     Assert.IsFalse(sanitizedXSS.Contains("<"), "Sanitized input should not contain HTML tags.");

        //     // Test SQL payload: Should remain unchanged (since Sanitize is for HTML, not SQL)
        //     string sqlPayload = "admin' --";
        //     string sanitizedSQL = DataValidator.Sanitize(sqlPayload);
        //     Assert.AreEqual("admin' --", sanitizedSQL, "SQL characters should not be altered by HTML sanitization.");

        //     // Test benign input: Should be preserved
        //     string benign = "<b>user</b>name";
        //     string sanitizedBenign = DataValidator.Sanitize(benign);
        //     Assert.AreEqual("username", sanitizedBenign, "Benign HTML should be stripped to safe text.");
        // }

        [Test]
        public void TestForXSS()
        {
            // Test raw malicious input against validation (sanitization is tested separately)
            var maliciousInput = new UserCreateSchema
            {
                Username = "<script>alert('XSS')</script>",  // Raw payload
                Email = "attacker@evil.com",
                Password = "Password123!"
            };

            var errors = DataValidator.ValidateSchema(maliciousInput);
            Assert.IsTrue(errors.ContainsKey("Username"), "XSS payload should be rejected by regex validation due to invalid characters.");
            // Optional: Check specific error messages
            Assert.IsTrue(errors["Username"].Any(e => e.Contains("must be 2-100 characters")), "Error should mention regex mismatch.");
        }
        [Test]
        public void TestForSQLInjection()
        {
            // Test raw malicious input against validation (sanitization is tested separately)
            var schema = new UserCreateSchema
            {
                Username = "admin' --",  // Raw payload
                Email = "test@safevault.com",
                Password = "Password123!"
            };
            var errors = DataValidator.ValidateSchema(schema);

            Assert.IsTrue(errors.ContainsKey("Username"), "SQL injection characters should violate the UsernamePattern.");
            // Optional: Ensure no false positives for valid inputs
            var validSchema = new UserCreateSchema { Username = "admin-user", Email = "test@safevault.com", Password = "Password123!" };
            var validErrors = DataValidator.ValidateSchema(validSchema);
            Assert.IsFalse(validErrors.ContainsKey("Username"), "Valid username should pass validation.");
        }
    }
}