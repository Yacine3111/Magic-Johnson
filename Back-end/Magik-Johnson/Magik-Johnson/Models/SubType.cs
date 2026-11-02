using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("SubTypes")]
public class SubType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<CardSubType> CardSubTypes { get; set; } = [];
}
