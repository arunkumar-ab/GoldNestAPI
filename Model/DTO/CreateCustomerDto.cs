using System.ComponentModel.DataAnnotations;

namespace GoldNest.Model.DTO
{
    public class CreateCustomerDto
    {
        
        public string CustomerName { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public int? Pincode { get; set; }
        public string? Area { get; set; } 
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    }
}