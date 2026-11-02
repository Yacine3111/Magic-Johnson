using System.Text.Json.Serialization;

namespace Magik_Johnson.DTOs.Requests;

public class ClientResetPasswordRequest
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("resetCode")]
    public required string ResetCode { get; set; }

    [JsonPropertyName("newPassword")]
    public required string NewPassword { get; set; }
}
