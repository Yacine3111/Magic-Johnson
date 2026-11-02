using System.Text.Json.Serialization;

namespace Magik_Johnson.DTOs.Requests;

public class ClientResetCodeCheckRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("resetCode")]
    public string ResetCode { get; set; } = default!;
}
