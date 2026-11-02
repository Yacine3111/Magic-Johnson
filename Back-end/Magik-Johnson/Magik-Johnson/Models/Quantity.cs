namespace Magik_Johnson.Models;

public class Quantity
{
    public Guid Id { get; set; }
    public Guid? LanguageId { get; set; }
    public Guid CardId { get; set; }
    public int NonFoilQuantity { get; set; }
    public int FoilQuantity { get; set; }
    public Language Language { get; set; } = null!;
    public Card Card { get; set; } = null!;
}
