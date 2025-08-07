using Employee.Api.Types;
using FluentValidation;

namespace Employee.Api.Validators;

public class BusinessEmployeeValidator : AbstractValidator<BusinessEmployee>
{
    public BusinessEmployeeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
            
        RuleFor(x => x.BankAccounts)
            .NotEmpty()
            .WithMessage("At least one bank account is required");
            
        RuleFor(x => x.BankAccounts)
            .Must(HaveValidPercentageTotal)
            .WithMessage("Bank account percentages must total exactly 100%")
            .When(x => x.BankAccounts.Any());
            
        RuleForEach(x => x.BankAccounts)
            .SetValidator(new BankAccountValidator());
    }
    
    private static bool HaveValidPercentageTotal(List<BankAccount> bankAccounts)
    {
        var total = bankAccounts.Sum(ba => ba.PayPercentage);
        return Math.Abs(total - 1.0m) < 0.001m; // Allow for small floating point differences
    }
}

public class BankAccountValidator : AbstractValidator<BankAccount>
{
    public BankAccountValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");
            
        RuleFor(x => x.RoutingNumber)
            .NotEmpty()
            .Matches(@"^\d{9}$")
            .WithMessage("Routing number must be exactly 9 digits");
            
        RuleFor(x => x.PayPercentage)
            .GreaterThan(0)
            .LessThanOrEqualTo(1)
            .WithMessage("Pay percentage must be between 0 and 100%");
    }
}