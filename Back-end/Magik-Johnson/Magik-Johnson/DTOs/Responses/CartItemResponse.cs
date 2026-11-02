using Magik_Johnson.DTOs.Cards;

namespace Magik_Johnson.DTOs.Responses;

public class CartItemResponse
{
    public CardResponse Card { get; set; }
    public int Quantity { get; set; }
    public string? Language { get; set; }
    public bool IsFoil { get; set; }
}
