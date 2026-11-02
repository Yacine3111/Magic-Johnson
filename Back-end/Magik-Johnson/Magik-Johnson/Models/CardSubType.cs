namespace Magik_Johnson.Models;

public class CardSubType
{
    public Guid CardId { get; set; }

    public Guid SubTypeId { get; set; }

    public Card Card { get; set; }
    public SubType SubType { get; set; }
}
