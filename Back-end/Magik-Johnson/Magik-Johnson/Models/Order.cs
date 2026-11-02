using Magik_Johnson.Enums;

namespace Magik_Johnson.Models;

public class Order
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? EmployeeId { get; set; }
    public string? Email { get; set; }
    public Guid OrderStatusId { get; set; }
    public string PaymentIntentId { get; set; } = default!;
    public OrderStatus OrderStatus { get; set; } = null!;
    public DateTimeOffset CheckoutDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset DeliveryDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxeAmount { get; set; }
    public ICollection<Refund> Refunds { get; set; }
    public ICollection<CardOrder> CardOrders { get; set; } = [];
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InStorePickup;
}
