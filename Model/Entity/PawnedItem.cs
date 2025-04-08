using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoldNest.Model.Entity
{
    public class PawnedItem
    {
        [Key]
        public int PawnedItemID { get; set; }

        [ForeignKey("Loan")]
        public required int LoanID { get; set; }

        [ForeignKey("Item")]
        public int ItemID { get; set; }

        public required decimal GrossWeight { get; set; }

        public required decimal NetWeight { get; set; }

        public string ItemType { get; set; }
        public decimal Amount { get; set; }

        // Navigation properties
        public Loan Loan { get; set; }
        public Item Item { get; set; }
    }
}