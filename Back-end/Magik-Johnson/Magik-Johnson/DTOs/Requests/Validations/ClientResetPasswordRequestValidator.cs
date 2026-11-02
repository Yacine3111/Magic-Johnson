using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class ClientResetPasswordRequestValidator : AbstractValidator<ClientResetPasswordRequest>
{
    public ClientResetPasswordRequestValidator()
    {
        RuleFor(cr => cr.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(cr => cr.ResetCode).NotNull().NotEmpty();
        RuleFor(cr => cr.NewPassword)
            .NotNull()
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;""'<>,.?/~`]).{8,}$"
            );
    }
}
