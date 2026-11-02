using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("Discounts")]
public class Discount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Description { get; set; }

    public int Rate { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public string Status { get; set; } = "En Attente";
}
