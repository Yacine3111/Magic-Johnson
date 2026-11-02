namespace Magik_Johnson.DTOs.Requests;

public class CartItemRequest
{
    public Guid CardId { get; set; }
    public int Quantity { get; set; }
    public string? Language { get; set; }
    public bool IsFoil { get; set; }
}
