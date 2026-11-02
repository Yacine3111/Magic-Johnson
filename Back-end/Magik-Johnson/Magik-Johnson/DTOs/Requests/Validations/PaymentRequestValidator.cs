using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class CreatePaymentRequestValidator : AbstractValidator<IEnumerable<PaymentRequest>>
{
    public CreatePaymentRequestValidator()
    {
        RuleForEach(p => p).SetValidator(new PaymentRequestValidator());
        RuleFor(p => p).NotEmpty();
    }
}

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(p => p.CardId).NotNull().NotEmpty();
        RuleFor(p => p.Price).NotNull().NotEmpty();
        RuleFor(p => p.CardName).NotNull().NotEmpty();
        RuleFor(p => p.ImageURL).NotNull().NotEmpty();
        RuleFor(p => p.Quantity).NotNull().NotEmpty();
    }
}
