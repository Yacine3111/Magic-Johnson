using System.Text.Json.Serialization;

namespace Magik_Johnson.DTOs.Requests;

public class EmployeeUpdateRequest
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [JsonPropertyName("name")]
    public required string LastName { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("phoneNumber")]
    public required string PhoneNumber { get; set; }
}
