using DTOs.Requests;
using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequest>
{
    public UpdatePasswordRequestValidator()
    {
        RuleFor(r => r.CurrentPassword)
            .NotNull()
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;""'<>,.?/~`]).{8,}$"
            );
        RuleFor(r => r.NewPassword)
            .NotNull()
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;""'<>,.?/~`]).{8,}$"
            );
        RuleFor(r => r.PasswordConfirmation).NotNull().Equal(r => r.NewPassword);
    }
}
