using Magik_Johnson.Enums;

namespace Magik_Johnson.DTOs.Requests;

public class CreatePendingPaymentDto
{
    public string PaymentIntentId { get; set; } = default!;
    public string Email { get; set; } = string.Empty;
    public string? UserId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public DeliveryMode DeliveryMode { get; set; }
    public IEnumerable<PaymentRequest> Items { get; set; } = default!;
}
