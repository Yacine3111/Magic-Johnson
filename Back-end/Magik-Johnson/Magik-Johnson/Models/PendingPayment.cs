using Magik_Johnson.Enums;

namespace Magik_Johnson.Models;

public class PendingPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public string PaymentIntentId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string? ShippingAddress { get; set; }
    public double ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ItemsJson { get; set; } = default!;
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InStorePickup;
}
