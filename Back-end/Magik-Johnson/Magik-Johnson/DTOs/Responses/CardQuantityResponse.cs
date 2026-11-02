namespace Magik_Johnson.DTOs.Cards;

public class CardQuantityResponse
{
    public Guid CardId { get; set; }
    public Guid QuantityId { get; set; }
    public Guid ScryfallId { get; set; }
    public Guid? SetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SetName { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? ManaCost { get; set; }
    public string? Text { get; set; }
    public string? Power { get; set; }
    public string? Toughness { get; set; }
    public decimal Price { get; set; }
    public decimal FoilPrice { get; set; }
    public bool HasFoil { get; set; }
    public bool HasNonFoil { get; set; }

    public Guid? LanguageId { get; set; }
    public string Language { get; set; } = string.Empty;
    public int NonFoilQuantity { get; set; }
    public int FoilQuantity { get; set; }
    public int TotalQuantity { get; set; }

    public List<string> Types { get; set; } = new();
    public List<string> SubTypes { get; set; } = new();
    public string Colors { get; set; } = string.Empty;
    public List<string> ArtistsNames { get; set; } = new();
}
