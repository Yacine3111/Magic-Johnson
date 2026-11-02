using System.Text.RegularExpressions;
using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class EmployeeRegistrationRequestValidator : AbstractValidator<EmployeeRegistrationRequest>
{
    public EmployeeRegistrationRequestValidator()
    {
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
        RuleFor(r => r.Password)
            .NotNull()
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;""'<>,.?/~`]).{8,}$"
            );
        RuleFor(r => r.PasswordConfirmation).NotNull().Equal(r => r.Password);
        RuleFor(r => r.HiringDate).NotNull();
        RuleFor(r => r.IsAccountEnabled).NotNull();
    }
}
