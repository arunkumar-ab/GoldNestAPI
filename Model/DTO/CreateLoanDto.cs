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
        public decimal? InterestAmount { get; set; }
        public string? Description { get; set; }
        public required DateTime LoanIssueDate { get; set; }
        public required List<CreatePawnedItemDto> PawnedItems { get; set; }
    }
    public class LoanDetailsDto
    {
        public int LoanID { get; set; }
        public string BillNo { get; set; }
        public DateTime LoanIssueDate { get; set; }
        public string Status { get; set; }
        public decimal InterestRate { get; set; }
        public decimal? AmountLoaned { get; set; }
        public CustomerDto Customer { get; set; }
        public List<PawnedItemDto> PawnedItems { get; set; }
        public LoanCalculationDto Calculation { get; set; }
    }

    public class CustomerDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string MobileNumber { get; set; }

        public string FatherName { get; set; }
        public string Address { get; set; }
        // Other properties you need, but NOT the Loans collection
    }

    public class PawnedItemDto
    {
        public int PawnedItemID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal grossWeight { get; set; }
        public decimal netWeight { get; set; }
        public decimal Amount { get; set; }
        // Other properties
    }

    public class LoanCalculationDto
    {
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int Months { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalPayable { get; set; }
    }
}
