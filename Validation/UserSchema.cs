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

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public required string Password { get; set; }
    }

    public class UserUpdateSchema
    {
        [RegularExpression(Constants.UsernamePattern, ErrorMessage = "Username must be 2-100 characters and can only contain letters, digits, and hyphens.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }

    public class UserLoginSchema
    {
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }
    }
}