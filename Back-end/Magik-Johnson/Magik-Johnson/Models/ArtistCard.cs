namespace Magik_Johnson.Models;

public class ArtistCard
{
    public Guid ArtistId { get; set; }

    public Guid CardId { get; set; }
    public Artist Artist { get; set; } = null!;
    public Card Card { get; set; } = null!;
}
