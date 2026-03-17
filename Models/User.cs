using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Annotations for validation attributes
namespace DotNetTutorial.Models
{
    public class User
    {
        [Key] // Marks this property as the primary key for database mapping
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(2, ErrorMessage = "Username must be at least 2 characters.")]
        [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User"; // Default role is "User"
    }
}
