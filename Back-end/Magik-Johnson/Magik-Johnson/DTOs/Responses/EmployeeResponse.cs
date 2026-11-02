using System.Text.Json.Serialization;

namespace DTOs.Responses;

public class EmployeeResponse
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

  
 
    [JsonPropertyName("isAccountLocked")]
    public bool IsAccountLocked { get; set; }

    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("hiringDate")]
    public DateTimeOffset? HiringDate { get; set; }
}

