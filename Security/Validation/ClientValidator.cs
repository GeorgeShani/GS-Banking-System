using Banking_System.Models;
using FluentValidation;

namespace Banking_System.Security.Validation;

public class ClientValidator : AbstractValidator<Client>
{
    public ClientValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().NotNull().WithMessage("First name is required")
            .Length(2, 50).WithMessage("First Name must be between 2 and 50 characters.");
        
        RuleFor(c => c.LastName)
            .NotEmpty().NotNull().WithMessage("Last name is required")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

        RuleFor(c => c.Username)
            .NotEmpty().NotNull().WithMessage("Username is required")
            .Length(5, 20).WithMessage("Username must be between 5 and 20 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");;
        
        RuleFor(c => c.PersonalNumber)
            .NotEmpty().WithMessage("Personal Number is required.")
            .Matches(@"^\d{11}$").WithMessage("Personal Number must be exactly 11 digits.");
        
        RuleFor(c => c.RegistrationDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Registration Date cannot be in the future.");
        
        RuleFor(c => c.Details)
            .NotNull().WithMessage("Client Details are required is required.")
            .SetValidator(new ClientDetailsValidator()!)
            .When(c => c.Details != null);
    }
}