using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("Types")]
public class Type
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<CardType> CardTypes { get; set; } = [];
}
