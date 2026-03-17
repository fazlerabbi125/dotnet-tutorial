using System.ComponentModel.DataAnnotations;
namespace DotNetTutorial.Validation
{

    public class UserCreateSchema
    {
        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression(Constants.UsernamePattern, ErrorMessage = "Username must be 2-100 characters and can only contain letters, digits, and hyphens.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
    }

    public class UserUpdateSchema
    {
        [RegularExpression(Constants.UsernamePattern, ErrorMessage = "Username must be 2-100 characters and can only contain letters, digits, and hyphens.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}