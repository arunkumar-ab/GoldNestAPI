using GoldNest.Model.DTO;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public static class ValidationHelper
{
    public static string ValidateUpdateCustomerData(CreateCustomerDto request)
    {
        // Name validation (At least 2 characters, only letters)
        if (string.IsNullOrWhiteSpace(request.CustomerName) || !Regex.IsMatch(request.CustomerName, @"^[A-Za-z]{2,}$"))
        {
            return "Customer name must contain only letters and be at least 2 characters long.";
        }

        if (string.IsNullOrWhiteSpace(request.FatherName) || !Regex.IsMatch(request.FatherName, @"^[A-Za-z]{2,}$"))
        {
            return "Father name must contain only letters and be at least 2 characters long.";
        }

        // Email validation
        if (!new EmailAddressAttribute().IsValid(request.Email))
        {
            return "Invalid email format.";
        }

        // India-specific phone number validation (Starts with 7, 8, or 9 followed by 9 digits)
        if (!Regex.IsMatch(request.MobileNumber, @"^[6789]\d{9}$"))
        {
            return "Phone number must be a valid 10-digit number starting with 7, 8, or 9.";
        }

        // Pincode validation
        if(!Regex.IsMatch(request.Pincode.ToString(), @"^\d{6}$"))
    {
            return "Pincode must be exactly 6 digits.";
        }

        if (string.IsNullOrWhiteSpace(request.Area) || !Regex.IsMatch(request.Area, @"^[A-Za-z]{2,}$"))
        {
            return "Area must contain only letters and be at least 2 characters long.";
        }

        return null; // No errors
    }


    public static string ValidateLoanData(CreateLoanDto request)
    {
        if (request == null)
        {
            return "Invalid loan request.";
        }

        // Validate customer details using existing helper
        string customerValidationError = ValidationHelper.ValidateUpdateCustomerData(request.Customer);
        if (customerValidationError != null)
        {
            return customerValidationError;
        }

        // Validate BillNo (First character must be a letter, followed by 1-4 digits)
        if (!Regex.IsMatch(request.BillNo, @"^[A-Za-z]\d{1,4}$"))
        {
            return "Invalid Bill Number format. It should start with a letter followed by up to 4 digits.";
        }

        // Validate AmountLoaned (Cannot be negative)
        if (request.AmountLoaned < 0)
        {
            return "Amount loaned cannot be negative.";
        }

        // Validate Pawned Items
        foreach (var item in request.PawnedItems)
        {
            if (item.GrossWeight < 0)
            {
                return $"Gross Weight cannot be negative for item {item.ItemID}.";
            }

            if (item.NetWeight < 0)
            {
                return $"Net Weight cannot be negative for item {item.ItemID}.";
            }
        }

        return null; // No validation errors
    }

}