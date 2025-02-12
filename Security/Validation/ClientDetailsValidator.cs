using Banking_System.Models;
using FluentValidation;

namespace Banking_System.Security.Validation;

public class ClientDetailsValidator : AbstractValidator<ClientDetails>
{
    public ClientDetailsValidator()
    {
        RuleFor(cd => cd.DateOfBirth)
            .LessThan(DateTime.Now).WithMessage("Date of Birth must be in the past.")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Date of Birth must not be more than 120 years ago.");

        RuleFor(cd => cd.Address)
            .NotEmpty().WithMessage("Address is required.")
            .Length(5, 100).WithMessage("Address must be between 5 and 100 characters.");
        
        RuleFor(cd => cd.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number is required.")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Phone Number must be between 10 and 15 digits and can start with '+'.");
        
        RuleFor(cd => cd.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
        
        RuleFor(cd => cd.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");
        
        RuleFor(cd => cd.IsVipClient)
            .NotNull().WithMessage("IsVipClient must be specified.");
        
        RuleFor(cd => cd.IsAdmin)
            .NotNull().WithMessage("IsAdmin must be specified.");
    }
}