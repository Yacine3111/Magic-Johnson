using Magik_Johnson.DTOs.Cards;

namespace Magik_Johnson.DTOs.Responses;

public class OrderItemResponse
{
    public CardResponse Card { get; set; }
    public int Quantity { get; set; }
    public bool IsFoil { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
