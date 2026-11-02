using System.Text.Json.Serialization;

namespace DTOs.Requests;

public class ClientForgotPasswordRequest
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }
}
