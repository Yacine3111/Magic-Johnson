namespace Magik_Johnson.DTOs.Requests;

public class DiscountRequestBase
{
    public Guid? Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public IEnumerable<Guid> CardsIds { get; set; } = [];

    public int Rate { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public string Status { get; set; } = "En Attente";
}
