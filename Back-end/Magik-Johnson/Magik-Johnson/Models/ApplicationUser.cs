using Microsoft.AspNetCore.Identity;

namespace Magik_Johnson.Models;

public class ApplicationUser : IdentityUser
{
    public string LastName { get; set; } = default!;

    public string FirstName { get; set; } = default!;

    public DateTimeOffset? HiringDate;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
