using System.Text.Json.Serialization;

namespace DTOs.Requests;

public class UpdateUserRequest
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = default!;

    [JsonPropertyName("name")]
    public string LastName { get; set; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = default!;
}
