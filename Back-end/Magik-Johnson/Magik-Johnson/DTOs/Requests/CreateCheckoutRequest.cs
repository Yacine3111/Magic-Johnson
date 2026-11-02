using System.Text.Json.Serialization;
using Magik_Johnson.Enums;

namespace Magik_Johnson.DTOs.Requests;

public class CreateCheckoutRequest
{
    [JsonPropertyName("itemsRequest")]
    public IEnumerable<PaymentRequest> ItemsRequest { get; set; } = [];

    [JsonPropertyName("shippingAddress")]
    public string? ShippingAddress { get; set; }

    [JsonPropertyName("shippingCost")]
    public decimal ShippingCost { get; set; }

    [JsonPropertyName("deliveryMode")]
    public DeliveryMode DeliveryMode { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; set; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }
}
