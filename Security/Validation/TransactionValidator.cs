using Banking_System.Models;
using FluentValidation;

namespace Banking_System.Security.Validation;

public class TransactionValidator : AbstractValidator<Transaction>
{
    public TransactionValidator()
    {
        RuleFor(t => t.Amount)
            .GreaterThan(0).WithMessage("Transaction amount must be greater than 0.");

        RuleFor(t => t.TransactionType)
            .IsInEnum().WithMessage("TransactionType must be a valid enum value.");
        
        RuleFor(t => t.TransactionDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("TransactionDate cannot be in the future.");
        
        RuleFor(t => t.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(255).WithMessage("Description must not exceed 255 characters.");
        
        RuleFor(t => t.Status)
            .IsInEnum().WithMessage("Status must be a valid enum value.");
    }
}