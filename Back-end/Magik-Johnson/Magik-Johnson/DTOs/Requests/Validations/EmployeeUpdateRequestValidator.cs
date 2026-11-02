using System.Text.RegularExpressions;
using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class EmployeeUpdateRequestValidator : AbstractValidator<EmployeeUpdateRequest>
{
    public EmployeeUpdateRequestValidator()
    {
        RuleFor(r => r.Id).NotNull().NotEmpty();

        RuleFor(r => r.FirstName)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .Matches(@"^[\p{L} ,.'-]+$", RegexOptions.IgnoreCase);
        RuleFor(r => r.LastName)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .Matches(@"^[\p{L} ,.'-]+$", RegexOptions.IgnoreCase);
        RuleFor(r => r.Email).NotNull().NotEmpty().Length(1, 100).EmailAddress();
        RuleFor(r => r.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .Length(1, 16)
            .Matches(@"^\(\d{3}\) \d{3} - \d{4}$");
    }
}
