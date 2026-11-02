namespace Magik_Johnson.Models;

public class Refund
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string? PaymentIntentId { get; set; }
    public double TotalAmount { get; set; }
    public double AmountToRefund { get; set; }
    public string Status { get; set; } = "en attente";
    public bool IsPartialRefund { get; set; }
    public Guid PreviousOrderStatusId { get; set; }
    public bool? IsAccepted { get; set; }
    public required string RefundJustification { get; set; }
    public string? Comments { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
