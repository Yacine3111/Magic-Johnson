namespace Magik_Johnson.Models;

public class Address
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public Guid? ProvinceId { get; set; }

    public Country? Country { get; set; }
    public Province? Province { get; set; }

    public int DoorNb { get; set; }

    public string? Unit { get; set; }

    public required string Street { get; set; }

    public required string City { get; set; }
    public required string PostalCode { get; set; }
    public string ApplicationUserId { get; set; } = default!;

    public ApplicationUser ApplicationUser { get; set; } = default!;
}
