using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions; // Required for sanitization

namespace DotNetTutorial.Validation
{
    public static class DataValidator
    {
        public static IDictionary<string, string[]> ValidateSchema(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(obj, context, results, validateAllProperties: true);

            Dictionary<string, List<string>> errors = new(StringComparer.OrdinalIgnoreCase);

            foreach (var validationResult in results)
            {
                var memberNames = validationResult.MemberNames.Any() 
                    ? validationResult.MemberNames 
                    : new[] { "__general__" };

                foreach (var memberName in memberNames)
                {
                    if (!errors.ContainsKey(memberName))
                    {
                        errors[memberName] = new List<string>();
                    }
                    errors[memberName].Add(validationResult.ErrorMessage ?? "Validation error.");
                }
            }

            return errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
        }

        // Sanitization logic to strip potentially harmful scripts and HTML
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // More comprehensive tag stripping
            string sanitized = Regex.Replace(input, "<[^>]*>", string.Empty);
            // Also encode to be sure
            return WebUtility.HtmlEncode(sanitized);
        }

        public static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }
    }
}