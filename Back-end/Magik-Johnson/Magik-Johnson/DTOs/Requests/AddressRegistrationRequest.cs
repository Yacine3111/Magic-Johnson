using System.Text.Json.Serialization;

namespace DTOs.Requests;

public class AddressRegistrationRequest
{
    [JsonPropertyName("addressNumber")]
    public required int DoorNb { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("street")]
    public required string Street { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("postalCode")]
    public required string PostalCode { get; set; }

    [JsonPropertyName("state")]
    public Guid ProvinceId { get; set; }

    [JsonPropertyName("country")]
    public Guid CountryId { get; set; }
}
