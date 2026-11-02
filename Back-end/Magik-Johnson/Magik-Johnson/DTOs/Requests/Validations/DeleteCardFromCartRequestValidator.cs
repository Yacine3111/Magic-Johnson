using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class DeleteCardFromCartRequestValidator : AbstractValidator<DeleteCardFromCartRequest>
{
    public DeleteCardFromCartRequestValidator()
    {
        RuleFor(d => d.LanguageName).NotNull().NotEmpty();
        RuleFor(d => d.IsFoil).NotNull();
    }
}
