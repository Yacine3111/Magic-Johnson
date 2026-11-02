using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("Carts")]
public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string IdUser { get; set; }

    public ICollection<CardCart> CardCarts { get; set; } = [];
}
