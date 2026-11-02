namespace Magik_Johnson.DTOs.Cards;

public class CardSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SetName { get; set; } = string.Empty;
    public Guid ScryfallId { get; set; }
}
