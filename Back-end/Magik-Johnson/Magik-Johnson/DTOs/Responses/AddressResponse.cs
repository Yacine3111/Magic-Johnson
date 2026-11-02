namespace Magik_Johnson.DTOs.Responses;

public class AddressResponse
{
    public Guid Id { get; set; }
    public string? Country { get; set; }
    public string? Province { get; set; }
    public Guid CountryId { get; set; }
    public Guid? ProvinceId { get; set; }

    public int DoorNb { get; set; }
    public string? Unit { get; set; }

    public required string Street { get; set; }

    public required string City { get; set; }
    public required string PostalCode { get; set; }
    public string UserId { get; set; } = default!;
}
