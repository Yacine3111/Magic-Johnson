using System.Text.Json.Serialization;

namespace DTOs.Requests;

public class UpdatePasswordRequest
{
    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; } = default!;

    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = default!;

    [JsonPropertyName("passwordConfirmation")]
    public string PasswordConfirmation { get; set; } = default!;
}
