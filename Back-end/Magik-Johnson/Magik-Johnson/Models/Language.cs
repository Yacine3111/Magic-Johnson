using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("Languages")]
public class Language
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardId { get; set; }
    public Guid ScryfallId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Card? Card { get; set; }
    public ICollection<Quantity> Quantities { get; set; } = new List<Quantity>();
    public ICollection<CardCart> CardCarts { get; set; } = [];
}
