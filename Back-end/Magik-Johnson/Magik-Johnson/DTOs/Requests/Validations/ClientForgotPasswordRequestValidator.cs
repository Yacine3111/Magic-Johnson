using DTOs.Requests;
using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class ClientForgotPasswordRequestValidator : AbstractValidator<ClientForgotPasswordRequest>
{
    public ClientForgotPasswordRequestValidator()
    {
        RuleFor(r => r.Email).NotNull().NotEmpty().Length(1, 100).EmailAddress();
    }
}
