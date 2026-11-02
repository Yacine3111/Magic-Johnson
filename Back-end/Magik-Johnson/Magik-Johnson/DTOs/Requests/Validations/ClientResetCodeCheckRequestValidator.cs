using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class ClientResetCodeCheckRequestValidator : AbstractValidator<ClientResetCodeCheckRequest>
{
    public ClientResetCodeCheckRequestValidator()
    {
        RuleFor(r => r.Email).NotNull().NotEmpty().Length(1, 100).EmailAddress();
        RuleFor(r => r.ResetCode).NotNull().NotEmpty();
    }
}
