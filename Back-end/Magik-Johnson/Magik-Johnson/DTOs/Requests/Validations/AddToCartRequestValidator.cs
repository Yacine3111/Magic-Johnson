using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(ac => ac.Items).NotEmpty().NotNull();
    }
}
