namespace Magik_Johnson.Models;

public class CardCart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardId { get; set; }

    public Guid CartId { get; set; }

    public Guid? LanguageId { get; set; }

    public int Quantity { get; set; }

    public bool IsFoil { get; set; } = false;

    public Card? Card { get; set; }

    public Cart? Cart { get; set; }

    public Language? Language { get; set; }
}
