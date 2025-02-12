using Banking_System.Models;
using FluentValidation;

namespace Banking_System.Security.Validation;

public class AccountValidator : AbstractValidator<Account>
{
    public AccountValidator()
    {
        RuleFor(a => a.AccountNumber)
            .NotEmpty().WithMessage("Account Number is required.")
            .Length(16).WithMessage("Account Number must be exactly 16 digits.")
            .Matches(@"^\d{16}$").WithMessage("Account Number must consist of 16 numeric digits.")
            .Must(PassChecksumValidation).WithMessage("Account Number is invalid (failed checksum validation).");
        
        RuleFor(a => a.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance must be non-negative.");

        RuleFor(a => a.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(BeValidCurrency).WithMessage("Currency must be one of the following: GEL, USD, EUR, GBP.");
        
        RuleFor(a => a.AccountType)
            .IsInEnum().WithMessage("Account Type must be a valid enum value (Current or Savings).");
        
        RuleFor(a => a.IsActive)
            .NotNull().WithMessage("IsActive must be specified.");
        
        RuleFor(a => a.OpenDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Open Date cannot be in the future.");
    }

    private static bool PassChecksumValidation(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) return false;

        var sum = 0;
        var doubleDigit = false;

        for (var i = accountNumber.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(accountNumber[i])) return false;

            var digit = accountNumber[i] - '0';
            if (doubleDigit)
            {
                digit *= 2;
            }

            sum += digit / 10;
            sum += digit % 10;
            
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    private static bool BeValidCurrency(string? currency)
    {
        string[] validCurrencies = ["GEL", "USD", "EUR", "GBP"];
        return validCurrencies.Contains(currency);
    }
}