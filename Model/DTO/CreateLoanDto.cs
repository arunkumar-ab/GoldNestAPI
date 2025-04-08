using System.ComponentModel.DataAnnotations.Schema;

namespace GoldNest.Model.DTO
{
    public class CreateLoanDto
    {
        public int CustomerID { get; set; }
        [ForeignKey("Loan")]
        public int LoanID { get; set; }
        public CreateCustomerDto? Customer { get; set; }
        public required string BillNo { get; set; }
        public required decimal AmountLoaned { get; set; }
        public string? Description { get; set; }
        public required DateTime LoanIssueDate { get; set; }
        public required List<CreatePawnedItemDto> PawnedItems { get; set; }
    }

}
