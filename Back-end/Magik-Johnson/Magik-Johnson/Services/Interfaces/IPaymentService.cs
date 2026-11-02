using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Enums;
using Stripe.Checkout;

namespace Magik_Johnson.Services.Interfaces;

public interface IPaymentService
{
    Task<Session> CreateCheckoutSessionAsync(
        IEnumerable<PaymentRequest> requests,
        DeliveryMode deliveryMode,
        decimal shippingCost,
        string? shippingAddress,
        string userEmail,
        HttpContext httpContext
    );

    Task SendReceiptAsync(string email);
}
