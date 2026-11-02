using Magik_Johnson.Enums;

namespace Magik_Johnson.DTOs.Responses;

public class OrderDetailResponse
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? ClientFirstName { get; set; }
    public string? ClientLastName { get; set; }
    public string? ClientEmail { get; set; }
    public Guid OrderStatusId { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;
    public DateTimeOffset CheckoutDate { get; set; }
    public DateTimeOffset DeliveryDate { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxeAmount { get; set; }
    public DeliveryMode DeliveryMode { get; set; }
}
