using FluentValidation;
using Magik_Johnson.Enums;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class CreateCheckoutRequestValidator : AbstractValidator<CreateCheckoutRequest>
{
    public CreateCheckoutRequestValidator()
    {
        RuleFor(x => x.ItemsRequest).NotEmpty().NotNull();
        RuleFor(x => x).Must(request => request.ItemsRequest.Sum(i => i.Price * i.Quantity) > 0.5m);

        RuleForEach(x => x.ItemsRequest).SetValidator(new PaymentRequestValidator());

        RuleFor(x => x.DeliveryMode).IsInEnum();
        When(
            x => x.DeliveryMode == DeliveryMode.HomeDelivery,
            () =>
            {
                RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(300);
            }
        );
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
