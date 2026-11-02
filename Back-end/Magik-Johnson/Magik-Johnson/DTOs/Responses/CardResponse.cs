using Magik_Johnson.DTOs.Responses;

namespace Magik_Johnson.DTOs.Cards;

public class CardResponse
{
    public Guid Id { get; set; }
    public Guid SetId { get; set; }
    public Guid ScryfallId { get; set; }

    public string SetName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ManaCost { get; set; } = string.Empty;
    public string? Text { get; set; } = string.Empty;
    public string? Power { get; set; }
    public string? Toughness { get; set; }
    public string? Colors { get; set; }
    public string Rarity { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public bool HasFoil { get; set; }
    public bool HasNonFoil { get; set; }
    public bool OnlineOnly { get; set; }
    public decimal Price { get; set; }
    public decimal FoilPrice { get; set; }
    public int Discount { get; set; }
    public int StockFoil { get; set; }
    public int StockNonFoil { get; set; }
    public int TotalQuantity { get; set; } = default;
    public int ClickCount { get; set; } = default;
    public List<string> Types { get; set; } = new();
    public List<string> SubTypes { get; set; } = new();
    public List<string> ArtistsNames { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<CardLanguageQuantity> LanguageQuantities { get; set; } = new();
}
