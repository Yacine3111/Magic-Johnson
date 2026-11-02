using System.Text.Json.Serialization;

namespace Magik_Johnson.DTOs.Requests;

public class PaymentRequest
{
    [JsonPropertyName("cardId")]
    public Guid CardId { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("cardName")]
    public required string CardName { get; set; }

    [JsonPropertyName("imageURL")]
    public required string ImageURL { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("isFoil")]
    public bool IsFoil { get; set; }

    [JsonPropertyName("language")]
    public required string Language { get; set; }
}
