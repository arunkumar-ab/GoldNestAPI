using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldNest.Model.Entity
{
    
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        
        public required string CustomerName { get; set; }

        public string? FatherName { get; set; }

        public string? Address { get; set; }

        public string? Area { get; set; }

        public int? Pincode { get; set; }

        public string? MobileNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Loan> Loans { get; set; }
    }
}