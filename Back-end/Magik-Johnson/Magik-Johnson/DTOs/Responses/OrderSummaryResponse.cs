using Magik_Johnson.Enums;
using Magik_Johnson.Models;

namespace Magik_Johnson.DTOs.Responses;

public class OrderSummaryResponse
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeFirstName { get; set; }
    public string? EmployeeLastName { get; set; }
    public string? Email { get; set; }
    public Guid OrderStatusId { get; set; }
    public OrderStatus OrderStatus { get; set; } = null!;
    public DateTimeOffset CheckoutDate { get; set; }
    public DateTimeOffset DeliveryDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxeAmount { get; set; }
    public DeliveryMode DeliveryMode { get; set; }
    public int TotalQuantity { get; set; }
}
