namespace Magik_Johnson.DTOs.Responses;

public class InventoryCardResponse
{
    public Guid Id { get; set; }
    public Guid ScryfallId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SetName { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal FoilPrice { get; set; }
    public string Couleur { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ManaCost { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Power { get; set; } = string.Empty;
    public string Toughness { get; set; } = string.Empty;
    public bool HasFoil { get; set; }
    public bool HasNonFoil { get; set; }
    public bool OnlineOnly { get; set; }

    // Quantités par langue
    public List<CardLanguageQuantity> LanguageQuantities { get; set; } = new();
}

public class CardLanguageQuantity
{
    public Guid LanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string FoilQuantity { get; set; } = "0";
    public string NonFoilQuantity { get; set; } = "0";
    public string TotalQuantity =>
        FoilQuantity == "N/A" || NonFoilQuantity == "N/A"
            ? "N/A"
            : (int.Parse(FoilQuantity) + int.Parse(NonFoilQuantity)).ToString();
}
