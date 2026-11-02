namespace Magik_Johnson.DTOs.Requests;

public class RefundRequest
{
    public required Guid RefundRequestId { get; set; }
    public required string PaymentIntentId { get; set; }
    public required Guid OrderId { get; set; }
    public required bool IsPartial { get; set; }
    public required bool IsAccepted { get; set; }
}
