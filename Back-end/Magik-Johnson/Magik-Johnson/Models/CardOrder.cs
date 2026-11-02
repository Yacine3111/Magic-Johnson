namespace Magik_Johnson.Models;

public class CardOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardId { get; set; }

    public Guid OrderId { get; set; }

    public int Quantity { get; set; }

    public bool IsFoil { get; set; }
    public double UnitPricePaid { get; set; }
    public bool IsRefunded { get; set; } = false;

    public Guid? LanguageId { get; set; }

    public Language? Language { get; set; }

    public Card? Card { get; set; }

    public Order? Order { get; set; }
}
