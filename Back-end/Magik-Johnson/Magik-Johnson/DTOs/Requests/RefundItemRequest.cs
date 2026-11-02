namespace Magik_Johnson.DTOs.Requests;

public class RefundItemRequest
{
    public Guid CardId { get; set; }
    public int Quantity { get; set; }
    public string? Language { get; set; }
}
