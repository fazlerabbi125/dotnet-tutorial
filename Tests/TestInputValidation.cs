using NUnit.Framework;
using DotNetTutorial.Validation;

namespace DotNetTutorial.Tests // Added namespace for good practice
{
    [TestFixture]
    public class TestInputValidation
    {
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

        [Test]
        public void TestSanitization()
        {
            string maliciousInput = "<script>alert('XSS')</script><b>Hello</b>";
            // Sanitize should strip tags
            string sanitized = DataValidator.Sanitize(maliciousInput);
            
            Assert.IsFalse(sanitized.Contains("<script>"), "Scripts should be stripped.");
            Assert.IsFalse(sanitized.Contains("<b>"), "HTML tags should be stripped.");
            Assert.AreEqual("alert(&#39;XSS&#39;)Hello", sanitized, "Should be stripped and encoded.");
        }

        [Test]
        public void TestEncode()
        {
            string maliciousInput = "<script>alert('XSS')</script>";
            string encoded = DataValidator.Encode(maliciousInput);
            
            Assert.AreEqual("&lt;script&gt;alert(&#39;XSS&#39;)&lt;/script&gt;", encoded, "Output should be HTML encoded.");
        }
        
        [Test]
        public void TestRejectsMaliciousChars()
        {
            var schema = new UserCreateSchema
            {
                Username = "admin' OR '1'='1",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var errors = DataValidator.ValidateSchema(schema);
            Assert.IsTrue(errors.ContainsKey("Username"), "Malicious SQL characters in username should be rejected by validation.");
        }
    }
}