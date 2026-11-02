namespace Magik_Johnson.Models;

public class Store
{
    public Guid Id { get; set; }

    public Guid AddressId { get; set; }

    public required string Email { get; set; }

    public required string Phone { get; set; }
}
