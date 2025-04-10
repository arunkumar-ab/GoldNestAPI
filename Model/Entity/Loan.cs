using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoldNest.Model.Entity
{
    public class Loan
    {
        [Key]
        public int LoanID { get; set; }

        public required string BillNo { get; set; }

        [ForeignKey("Customer")]
        public int CustomerID { get; set; }

        public required DateTime LoanIssueDate { get; set; }

        public required string Status { get; set; } = "Open"; // Open/Closed

        public required decimal InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }

        public DateTime? CloseDate { get; set; }
        public required decimal AmountLoaned { get; set; }

        public string Description { get; set; }
        // Navigation properties
        public Customer Customer { get; set; }
        public ICollection<PawnedItem> PawnedItems { get; set; }
    }
}