using System.ComponentModel.DataAnnotations;

namespace GoldNest.Model.Entity
{
    public class User
    {
        public int UserID { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public required string Role { get; set; } = "Staff"; // Admin/Staff

        public DateTime? LastLogin { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}