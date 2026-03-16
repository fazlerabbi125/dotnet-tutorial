using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNetTutorial.Models
{
    // DTO for accepting user input (includes password)
    public class UserInput
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public required string Gender { get; set; }
    }

    public class User
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [JsonIgnore]
        public string Password { get; set; } = null!;

        private string _gender = null!;
        [Required(ErrorMessage = "Gender is required.")]
        public required string Gender {
            get => _gender;
            set {
                string[] allowedValues = ["male", "female", "others"];
                if (!allowedValues.Contains(value))
                {
                    throw new ArgumentException("Invalid gender value. Allowed values: male, female, others.");
                }
                _gender = value;
            }
        }
    }
}
