
using GoldNest.Data;
using GoldNest.Model.Entity;
using Microsoft.AspNetCore.Mvc;
using GoldNest.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GoldNest.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<LoanController> _logger;

        public LoanController(ApplicationDbContext dbContext, ILogger<LoanController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        //Get customer details
        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            try
            {
                var customers = await _dbContext.Customer.ToListAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers");
                return StatusCode(500, new { message = "An error occurred while fetching customers.", error = ex.Message });
            }
        }
        [HttpGet("loanhistory/{id}")]
        public async Task<ActionResult<IEnumerable<Loan>>> GetCustomerLoans(int id)
        {
            try
            {
                // Check if customer exists first
                var customerExists = await _dbContext.Customer.AnyAsync(c => c.CustomerID == id);
                if (!customerExists)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                // Get all loans for the customer
                var loans = await _dbContext.Loan
                    .Where(l => l.CustomerID == id)
                    .Select(l => new
                    {
                        Loanid = l.LoanID,
                        loanAmount = l.AmountLoaned,
                        interestRate = l.InterestRate,
                        status = l.Status,
                        date = l.LoanIssueDate.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching loans for customer {CustomerId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching the customer's loans.", error = ex.Message });
            }
        }
        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customer = new Customer
                {

                    CustomerName = request.CustomerName,
                    FatherName = request.FatherName,
                    Address = request.Address,
                    Pincode = request.Pincode,
                    MobileNumber = request.MobileNumber,
                    Email = request.Email
                };

                _dbContext.Customer.Add(customer);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerID }, customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the customer.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _dbContext.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found." });
            }
            return Ok(customer);
        }

        //Update customer details
        [HttpPut("customer/{id:int}")]
        public async Task<ActionResult<Customer>> UpdateCustomer(int id, [FromBody] CreateCustomerDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid customer data" });
            }

            try
            {
                var existingCustomer = await _dbContext.Customer.FindAsync(id);
                if (existingCustomer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }

                var validationError = ValidationHelper.ValidateUpdateCustomerData(request);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                // Update fields
                existingCustomer.CustomerName = request.CustomerName;
                existingCustomer.FatherName = request.FatherName;
                existingCustomer.Email = request.Email;
                existingCustomer.MobileNumber = request.MobileNumber;
                existingCustomer.Address = request.Address;
                existingCustomer.Area = request.Area;
                existingCustomer.Pincode = request.Pincode;

                await _dbContext.SaveChangesAsync();

                return Ok(existingCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer {id}");
                return StatusCode(500, new { message = "An error occurred while updating the customer.", error = ex.Message });
            }
        }

        [HttpPost("createloan")]
        public async Task<ActionResult<Loan>> CreateLoan([FromBody] CreateLoanDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid loan data" });
            }
            _logger.LogInformation("Entering the create loan backend");
            try
            {
                _logger.LogInformation("checking customer selected or not ");
                // If CustomerID is provided, verify the customer exists
                // Before creating new customer
                
                if (request.CustomerID > 0)
                {
                    _logger.LogInformation("Customer exists");
                    var customerExists = await _dbContext.Customer.AnyAsync(c => c.CustomerID == request.CustomerID);
                    if (!customerExists)
                    {
                        return BadRequest(new { message = "Customer not found with provided ID" });
                    }
                }

                else
                {
                    _logger.LogInformation("Creating new Customer");
                    var validationError = ValidationHelper.ValidateLoanData(request);
                    if (validationError != null)
                    {
                        return BadRequest(new { message = validationError });
                    }
                    // Create and save new customer
                    var customer = new Customer
                    {
                        CustomerName = request.Customer.CustomerName,
                        FatherName = request.Customer.FatherName,
                        MobileNumber = request.Customer.MobileNumber,
                        Address = request.Customer.Address,
                        Pincode = request.Customer.Pincode,
                        Email = request.Customer.Email,
                        Area = request.Customer.Area
                    };
                    _logger.LogInformation("Created Customer");
                    await _dbContext.Customer.AddAsync(customer);
                    await _dbContext.SaveChangesAsync();
                    request.CustomerID = customer.CustomerID; // Update the DTO with new CustomerID
                }

                // Create and save loan
                var loan = new Loan
                {
                    BillNo = request.BillNo,
                    CustomerID = request.CustomerID, // We know it has value at this point
                    Status = "active",
                    InterestRate = 1,
                    LoanIssueDate = request.LoanIssueDate,
                    AmountLoaned = request.AmountLoaned,
                    Description = request.Description
                };

                await _dbContext.Loan.AddAsync(loan);
                await _dbContext.SaveChangesAsync();

                // Create and save pawned items
                var pawnedItems = request.PawnedItems.Select(item => new PawnedItem
                {
                    ItemID = item.ItemID,
                    ItemType = item.ItemType,
                    LoanID = loan.LoanID,
                    GrossWeight = item.GrossWeight,
                    NetWeight = item.NetWeight,
                    Amount = item.Amount
                }).ToList();

                await _dbContext.PawnedItems.AddRangeAsync(pawnedItems);
                await _dbContext.SaveChangesAsync();


                return Ok(new { message = "Loan created successfully", loanID = loan.LoanID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loan");
                return StatusCode(500, new { message = "An error occurred while creating the loan", error = ex.Message });
            }
        }
        //Get all loans
        [HttpGet("loans")]
        public async Task<ActionResult> GetLoans(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string status = "all",
            [FromQuery] string search = null)
        {
            try
            {
                var query = _dbContext.Loan
                    .Include(l => l.Customer)
                    .AsQueryable();

                // Apply filters
                if (fromDate.HasValue)
                    query = query.Where(l => l.LoanIssueDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(l => l.LoanIssueDate <= toDate.Value);

                if (status.ToLower() != "all")
                    query = query.Where(l => l.Status.ToLower() == status.ToLower());

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(l =>
                        l.BillNo.Contains(search) ||
                        l.Customer.CustomerName.Contains(search));

                var allLoans = await query.ToListAsync();
                var closedLoans = allLoans.Where(l => l.CloseDate != null).ToList();

                // Calculations (only for closed loans)
                decimal totalPrincipalReceived = closedLoans.Sum(l => l.AmountLoaned);
                decimal totalInterestReceived = closedLoans.Sum(l =>
                    l.AmountLoaned * 0.01m * CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value));

                var loanResults = allLoans.Select(l => new
                {
                    l.LoanID,
                    l.BillNo,
                    CustomerId = l.Customer.CustomerID,
                    CustomerName = l.Customer.CustomerName,
                    OpenDate = l.LoanIssueDate,
                    l.CloseDate,
                    Amount = l.AmountLoaned,
                    l.Status,
                    InterestMonths = l.CloseDate != null ?
                        CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value) : (int?)null,
                    Interest = l.CloseDate != null ?
                        l.AmountLoaned * 0.01m * CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value) : (decimal?)null,
                    AmountReceived = l.CloseDate != null ?
                        l.AmountLoaned + (l.AmountLoaned * 0.01m * CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value))
                        : (decimal?)null
                }).ToList();

                var result = new
                {
                    Loans = loanResults,
                    Summary = new
                    {
                        TotalLoanAmount = allLoans.Sum(l => l.AmountLoaned),
                        TotalPrincipalReceived = totalPrincipalReceived,
                        TotalInterestReceived = totalInterestReceived,
                        TotalAmountReceived = totalPrincipalReceived + totalInterestReceived,
                        ActiveLoansCount = allLoans.Count(l => l.CloseDate == null),
                        ClosedLoansCount = closedLoans.Count
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching loans");
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching loans.",
                    error = ex.Message
                });
            }
        }

        // POST api/close/{id}
        [HttpPost("close/{id}")]
        public async Task<ActionResult<object>> CloseLoan(int id)
        {
            try
            {
                // Find the loan with the given ID
                var loan = await _dbContext.Loan
                    .FirstOrDefaultAsync(l => l.LoanID == id);

                if (loan == null)
                {
                    return NotFound(new { message = "Loan not found" });
                }

                // Get loan calculation details
                var today = DateTime.Now;
                var startDate = loan.LoanIssueDate;
                var interestMonths = CalculateInterestMonths(startDate, today);
                var principal = loan.AmountLoaned;
                var interestRate = loan.InterestRate;
                var interestAmount = principal * (interestRate / 100) * interestMonths;
                var totalPayable = principal + interestAmount;

                // Update the loan with payment details
                loan.Status = "closed";
                loan.CloseDate = DateTime.Now;
                loan.InterestAmount = interestAmount;

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Loan closed successfully",
                    loanId = id,
                    amountPaid = totalPayable,
                    interestAmount = interestAmount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while closing the loan", error = ex.Message });
            }
        }

        //[HttpGet("loans/{id}")]
        //public async Task<ActionResult<object>> GetLoanDetails(int id)
        //{
        //    try
        //    {
        //        // Find the loan with the given ID
        //        var loan = await _dbContext.Loan
        //            .Include(l => l.Customer)
        //            .FirstOrDefaultAsync(l => l.LoanID == id);

        //        if (loan == null)
        //        {
        //            return NotFound(new { message = "Loan not found" });
        //        }

        //        // Find all pawned items for this loan
        //        var loanItems = await _dbContext.PawnedItems
        //        .Where(item => item.LoanID == id)
        //        .Include(item => item.Item) // Explicit include
        //        .AsNoTracking() // Optional for read-only
        //         .ToListAsync();

        //        // Format loan data in the requested format
        //        var loanData = new
        //        {
        //            id = loan.BillNo,
        //            customerId = loan.CustomerID.ToString(),
        //            customerName = loan.Customer.CustomerName,
        //            date = loan.LoanIssueDate.ToString("yyyy-MM-dd"),
        //            closedDate = loan.CloseDate.HasValue ? loan.CloseDate.Value.ToString("yyyy-MM-dd") : "",
        //            amount = "₹" + loan.AmountLoaned.ToString("N0"),
        //            status = loan.Status
        //        };

        //        // Format customer data
        //        var customerData = new
        //        {
        //            id = loan.Customer.CustomerID.ToString(),
        //            name = loan.Customer.CustomerName,
        //            phone = loan.Customer.MobileNumber,
        //            email = loan.Customer.Email,
        //            address = loan.Customer.Address,
        //        };

        //        // Format pawned items data
        //        var itemsData = loanItems.Select(item => new
        //        {
        //            loanId = loan.BillNo,
        //            name = item.Item?.ItemName ?? "Unknown Item",
        //            grossWeight = item.GrossWeight,
        //            netWeight = item.NetWeight,
        //            value = item.Amount
        //        }).ToList();

        //        // Return the complete loan details exactly matching the required format
        //        return Ok(new
        //        {
        //            loan,
        //            customer = customerData,
        //            loanItems
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while fetching loan details", error = ex.Message });
        //    }
        //}

        // GET api/loans/search
        [HttpGet("loans/search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchLoansByBillNo([FromQuery] string billNo)
        {
            try
            {
                if (string.IsNullOrEmpty(billNo))
                {
                    return BadRequest(new { message = "Bill number is required" });
                }

                // Search for loans with matching bill number
                var loans = await _dbContext.Loan
                    .Include(l => l.Customer)
                    .Where(l => l.BillNo.Contains(billNo))
                    .OrderByDescending(l => l.LoanIssueDate)
                    .ToListAsync();

                if (loans == null || !loans.Any())
                {
                    return NotFound(new { message = "No loans found with the provided bill number" });
                }

                // Format the loans for the response
                var formattedLoans = loans.Select(loan => new
                {
                    loan.LoanID,
                    loan.BillNo,
                    loan.Customer.CustomerName,
                    LoanIssueDate = loan.LoanIssueDate.ToString("yyyy-MM-dd"),
                    AmountLoaned = "₹" + loan.AmountLoaned.ToString("N0"),
                    status = loan.Status,
                    customerID = loan.CustomerID
                });

                return Ok(formattedLoans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching for loans", error = ex.Message });
            }
        }


        //Get items table 
        [HttpGet("items")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch items from database");

                var items = await _dbContext.Item.ToListAsync();
                if (!items.Any())
                {
                    return NotFound(new { message = "No items found." });
                }
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items");
                return StatusCode(500, new { message = "An error occurred while fetching items.", error = ex.Message });
            }
        }


        [HttpGet("loans/{id}")]
        public async Task<ActionResult<LoanDetailsDto>> GetLoanDetails(int id)
        {
            try
            {
                var loan = await _dbContext.Loan
                    .Include(l => l.Customer)
                    .FirstOrDefaultAsync(l => l.LoanID == id);

                if (loan == null)
                {
                    return NotFound(new { message = "Loan not found" });
                }

                var loanItems = await _dbContext.PawnedItems
                    .Include(p => p.Item)
                    .Where(item => item.LoanID == id)
                    .ToListAsync();

                var today = DateTime.Now;
                var interestMonths = CalculateInterestMonths(loan.LoanIssueDate, today);
                var principal = loan.AmountLoaned;
                var interestAmount = principal * (loan.InterestRate / 100) * interestMonths;

                return new LoanDetailsDto
                {
                    LoanID = loan.LoanID,
                    BillNo = loan.BillNo,
                    LoanIssueDate = loan.LoanIssueDate,
                    Status = loan.Status,
                    InterestRate = loan.InterestRate,
                    AmountLoaned = loan.AmountLoaned,
                    Customer = new CustomerDto
                    {
                        CustomerID = loan.Customer.CustomerID,
                        CustomerName = loan.Customer.CustomerName,
                        FatherName = loan.Customer.FatherName,
                        Address = loan.Customer.Address,
                        MobileNumber = loan.Customer.MobileNumber
                        // Other properties
                    },
                    PawnedItems = loanItems.Select(item => new PawnedItemDto
                    {
                        PawnedItemID = item.PawnedItemID,
                        ItemID = item.ItemID,
                        ItemName = item.Item.ItemName,
                        grossWeight = item.GrossWeight,
                        netWeight = item.NetWeight,
                        Amount =item.Amount

                        // Other properties
                    }).ToList(),
                    Calculation = new LoanCalculationDto
                    {
                        Principal = principal,
                        InterestRate = loan.InterestRate,
                        Months = interestMonths,
                        InterestAmount = interestAmount,
                        TotalPayable = principal + interestAmount
                    }
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching loan details", error = ex.Message });
            }
        }

        // Helper method to calculate interest months
        private static int CalculateInterestMonths(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate) return 0;

            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            // First month is always counted
            if (months == 0) return 1;

            // After first month, count additional month if it's more than or equal to 2 days
            if (endDate.Day > startDate.Day + 2 ||
                (endDate.Day >= startDate.Day && endDate > startDate.AddMonths(months)))
            {
                months++;
            }

            return Math.Max(1, months);
        }

    }
}