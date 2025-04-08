//using GoldNest.Data;
//using GoldNest.Model.Entity;
//using Microsoft.AspNetCore.Mvc;
//using GoldNest.Model.DTO;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;


//namespace GoldNest.Controllers
//{
//    [Route("api/")]
//    [ApiController]
//    public class LoanController : ControllerBase
//    {
//        private readonly ApplicationDbContext dbContext;
//        private readonly ILogger<LoanController> _logger;

//        public LoanController(ApplicationDbContext dbContext, ILogger<LoanController> logger)
//        {
//            this.dbContext = dbContext;
//            _logger = logger;
//        }

//        [HttpGet]
//        [Route("customers")]
//        public IActionResult GetCustomers()
//        {
//            try
//            {
//                var CustomersList = dbContext.Customer.ToList();
//                return Ok(CustomersList);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "An error occurred while fetching customers.", error = ex.Message });
//            }
//        }

//        // GET: api/item
//        [HttpGet("Items")]
//        public IActionResult GetItems()
//        {
//            try
//            {
//                var ItemsList = dbContext.Items.ToList();
//                if (!ItemsList.Any())
//                {
//                    return NotFound(new { message = "No items found." });
//                }

//                return Ok(ItemsList);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "An error occurred while fetching Items.", error = ex.Message });
//            }
//        }

//        [HttpPut("customer/{id:int}")]
//        public IActionResult UpdateCustomer(int id, [FromBody] CreateCustomerDto request)
//        {
//            if (request == null)
//            {
//                return BadRequest(new { message = "Invalid customer data" });
//            }

//            try
//            {
//                var existingCustomer = dbContext.Customer.Find(id);
//                if (existingCustomer == null)
//                {
//                    return NotFound(new { message = "Customer not found" });
//                }

//                var validationError = ValidationHelper.ValidateUpdateCustomerData(request);
//                if (validationError != null)
//                {
//                    return BadRequest(new { message = validationError });
//                }

//                // Update fields only if valid data is provided
//                existingCustomer.CustomerName = request.CustomerName;
//                existingCustomer.FatherName = request.FatherName;
//                existingCustomer.Email = request.Email;
//                existingCustomer.MobileNumber = request.MobileNumber;
//                existingCustomer.Address = request.Address;
//                existingCustomer.Area = request.Area;
//                existingCustomer.Pincode= request.Pincode;

//                dbContext.SaveChanges();

//                return Ok(existingCustomer);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "An error occurred while updating the customer.", error = ex.Message });
//            }
//        }


//        [HttpPost("createloan")]
//        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanDto request)
//        {
//            if (request == null)
//            {
//                return BadRequest(new { message = "Invalid loan data" });
//            }
//            var validationError = ValidationHelper.ValidateLoanData(request);
//            if (validationError != null)
//            {
//                return BadRequest(new { message = validationError });
//            }
//            using var transaction = await dbContext.Database.BeginTransactionAsync();
//            try
//            {
//                // Create and save customer
//                var customer = new Customer
//                {
//                    CustomerName = request.Customer.CustomerName,
//                    FatherName = request.Customer.FatherName,
//                    MobileNumber = request.Customer.MobileNumber,
//                    Address = request.Customer.Address,
//                    Pincode = request.Customer.Pincode,
//                    Email = request.Customer.Email,
//                    Area = request.Customer.Area
//                };
//                dbContext.Customer.Add(customer);
//                await dbContext.SaveChangesAsync();

//                // Create and save loan
//                var loan = new Loan
//                {
//                    BillNo = request.BillNo,
//                    CustomerID = customer.CustomerID,
//                    Status = "active",
//                    InterestRate = 1,
//                    LoanIssueDate = request.LoanIssueDate,
//                    AmountLoaned = request.AmountLoaned,
//                    Description = request.Description
//                };
//                dbContext.Loan.Add(loan);
//                await dbContext.SaveChangesAsync();

//                // Create and save pawned items
//                var pawnedItems = request.PawnedItems.Select(item => new PawnedItem
//                {
//                    ItemID = item.ItemID,
//                    ItemType = item.ItemType,
//                    LoanID = loan.LoanID,
//                    GrossWeight = item.GrossWeight,
//                    NetWeight = item.NetWeight
//                }).ToList();

//                dbContext.PawnedItems.AddRange(pawnedItems);
//                await dbContext.SaveChangesAsync();

//                await transaction.CommitAsync();

//                return Ok(new { message = "Loan created successfully", loanID = loan.LoanID });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                return StatusCode(500, new { message = "An error occurred while creating the loan", error = ex.Message });
//            }
//        }

//        [HttpGet("loans")]
//        public IActionResult GetLoans(
//    [FromQuery] DateTime? fromDate,
//    [FromQuery] DateTime? toDate,
//    [FromQuery] string status = "all",
//    [FromQuery] string search = null)
//        {
//            try
//            {
//                var query = dbContext.Loan
//                    .Include(l => l.Customer)
//                    .AsQueryable();

//                // Apply filters
//                if (fromDate.HasValue)
//                    query = query.Where(l => l.LoanIssueDate >= fromDate.Value);

//                if (toDate.HasValue)
//                    query = query.Where(l => l.LoanIssueDate <= toDate.Value);

//                if (status.ToLower() != "all")
//                    query = query.Where(l => l.Status.ToLower() == status.ToLower());

//                if (!string.IsNullOrEmpty(search))
//                    query = query.Where(l =>
//                        l.BillNo.Contains(search) ||
//                        l.Customer.CustomerName.Contains(search));

//                var allLoans = query.ToList();
//                var closedLoans = allLoans.Where(l => l.CloseDate != null).ToList();

//                // CALCULATIONS (ONLY FOR CLOSED LOANS)
//                decimal totalPrincipalReceived = closedLoans.Sum(l => l.AmountLoaned);
//                decimal totalInterestReceived = closedLoans.Sum(l =>
//                    l.AmountLoaned * 0.01m * CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value));

//                var loanResults = allLoans.Select(l => new
//                {
//                    l.BillNo,
//                    CustomerName = l.Customer.CustomerName,
//                    OpenDate = l.LoanIssueDate,
//                    l.CloseDate,
//                    Amount = l.AmountLoaned,
//                    l.Status,
//                    // Only show for closed loans
//                    InterestMonths = l.CloseDate != null ?
//                        CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value) : (int?)null,
//                    Interest = l.CloseDate != null ?
//                        l.AmountLoaned * 0.01m * CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value) : (decimal?)null,
//                    // New: Amount received (only for closed loans)
//                    AmountReceived = l.CloseDate != null ?
//                        l.AmountLoaned + (l.AmountLoaned * 0.01m * CalculateInterestMonths(l.LoanIssueDate, l.CloseDate.Value))
//                        : (decimal?)null
//                }).ToList();

//                var result = new
//                {
//                    Loans = loanResults,
//                    Summary = new
//                    {
//                        TotalLoanAmount = allLoans.Sum(l => l.AmountLoaned), // All loans
//                        TotalPrincipalReceived = totalPrincipalReceived,     // Only closed
//                        TotalInterestReceived = totalInterestReceived,       // Only closed
//                        TotalAmountReceived = totalPrincipalReceived + totalInterestReceived, // Only closed
//                        ActiveLoansCount = allLoans.Count(l => l.CloseDate == null),
//                        ClosedLoansCount = closedLoans.Count
//                    }
//                };

//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    message = "An error occurred while fetching loans.",
//                    error = ex.Message
//                });
//            }
//        }
//        private int CalculateInterestMonths(DateTime startDate, DateTime endDate)
//        {
//            if (endDate < startDate) return 0;

//            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

//            // Add partial month if >2 days into next month
//            if (endDate.Day > startDate.Day + 2 ||
//                (endDate.Day >= startDate.Day && endDate > startDate.AddMonths(months)))
//            {
//                months++;
//            }

//            return Math.Max(1, months); // Minimum 1 month interest
//        }
//        [HttpPut("close/{loanId}")]
//        public async Task<IActionResult> CloseLoan(
//            [FromRoute] int loanId,
//            [FromBody] CloseLoanRequest request)
//        {
//            try
//            {
//                // Find the loan by ID
//                var loan = await dbContext.Loan.FindAsync(loanId);

//                if (loan == null)
//                {
//                    return NotFound(new { message = "Loan not found." });
//                }
//                // Check if loan is already closed
//                if (loan.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
//                {
//                    return BadRequest(new { message = "Loan is already closed." });
//                }

//                // Update loan status and close date
//                loan.Status = "closed";
//                loan.CloseDate = request.CloseDate;

//                dbContext.Loan.Update(loan);
//                await dbContext.SaveChangesAsync();

//                return Ok(new { message = "Loan closed successfully." });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    message = "An error occurred while closing the loan.",
//                    error = ex.Message
//                });
//            }
//        }


//    }
//}
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

        //Close loan
        [HttpPut("close/{loanId}")]
        public async Task<ActionResult> CloseLoan(
            [FromRoute] int loanId,
            [FromBody] CloseLoanRequest request)
        {
            try
            {
                var loan = await _dbContext.Loan.FindAsync(loanId);
                if (loan == null)
                {
                    return NotFound(new { message = "Loan not found." });
                }

                if (loan.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Loan is already closed." });
                }

                loan.Status = "closed";
                loan.CloseDate = request.CloseDate;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Loan closed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing loan {loanId}");
                return StatusCode(500, new
                {
                    message = "An error occurred while closing the loan.",
                    error = ex.Message
                });
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
        // Add a new item to the database
        [HttpPost("items")]
        public async Task<ActionResult<Item>> AddItem([FromBody] Item newItem)
        {
            try
            {
                _logger.LogInformation("Attempting to add new item to database");

                if (newItem == null)
                {
                    return BadRequest(new { message = "Invalid item data." });
                }

                await _dbContext.Item.AddAsync(newItem);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetItems), new { id = newItem.ItemID }, newItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item");
                return StatusCode(500, new { message = "An error occurred while adding the item.", error = ex.Message });
            }
        }


        private static int CalculateInterestMonths(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate) return 0;

            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            if (endDate.Day > startDate.Day + 2 ||
                (endDate.Day >= startDate.Day && endDate > startDate.AddMonths(months)))
            {
                months++;
            }

            return Math.Max(1, months);
        }
    }
}