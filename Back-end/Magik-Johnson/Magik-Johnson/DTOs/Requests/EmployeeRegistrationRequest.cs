using System.Text.Json.Serialization;

namespace Magik_Johnson.DTOs.Requests;

public class EmployeeRegistrationRequest
{
    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [JsonPropertyName("name")]
    public required string LastName { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("phoneNumber")]
    public required string PhoneNumber { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }

    [JsonPropertyName("passwordConfirmation")]
    public required string PasswordConfirmation { get; set; }

    [JsonPropertyName("hiringDate")]
    public DateTimeOffset HiringDate { get; set; }

    [JsonPropertyName("isAccountEnabled")]
    public bool IsAccountEnabled { get; set; }
}
