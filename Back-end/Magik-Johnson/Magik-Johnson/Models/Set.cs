using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("Sets")]
public class Set
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SetCode { get; set; } = string.Empty;
}
