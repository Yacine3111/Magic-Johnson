using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class DiscountRequestValidator : AbstractValidator<DiscountRequestBase>
{
    public DiscountRequestValidator()
    {
        RuleFor(d => d.Title).NotEmpty().NotNull().MaximumLength(100);
        RuleFor(d => d.Description).NotEmpty().NotNull().MaximumLength(300);
        RuleFor(d => d.CardsIds).NotEmpty().NotNull();
        RuleFor(d => d.Rate).NotNull().InclusiveBetween(1, 100);
        RuleFor(d => d.StartDate).NotNull();
        RuleFor(d => d.EndDate).NotNull().GreaterThan(d => d.StartDate);
    }
}
