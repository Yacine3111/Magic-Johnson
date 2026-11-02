using System.Text.Json.Serialization;

namespace DTOs.Requests;

public class ClientRegistrationRequest
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = default!;

    [JsonPropertyName("name")]
    public string LastName { get; set; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = default!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = default!;

    [JsonPropertyName("passwordConfirmation")]
    public string PasswordConfirmation { get; set; } = default!;
}
