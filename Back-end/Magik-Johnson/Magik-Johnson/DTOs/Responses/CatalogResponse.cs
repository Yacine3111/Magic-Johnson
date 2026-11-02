namespace Magik_Johnson.DTOs.Responses;

public class CatalogResponse
{
    public Guid Id { get; set; }
    public Guid ScryfallId { get; set; }
    public required string Name { get; set; }
    public required string SetName { get; set; }
    public required string Rarity { get; set; }
    public required string Number { get; set; }
    public decimal Price { get; set; }
    public decimal FoilPrice { get; set; }
    public int Discount { get; set; }
    public bool HasFoil { get; set; }
    public bool HasNonFoil { get; set; }
    public bool OnlineOnly { get; set; }
    public int StockFoil { get; set; }
    public int StockNonFoil { get; set; }
    public int TotalQuantity { get; set; }
}
