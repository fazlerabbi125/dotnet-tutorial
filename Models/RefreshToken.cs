using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetTutorial.Models
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserID { get; set; } // Foreign key

        public required string Token { get; set; }

        public DateTime ExpiryTime { get; set; }

        public bool IsRevoked { get; set; } = false;
    }
}
