namespace Magik_Johnson.Models;

public class CardType
{
    public Guid CardId { get; set; }

    public Guid TypeId { get; set; }

    public Card Card { get; set; } = null!;
    public Type Type { get; set; } = null!;
}
