using System.ComponentModel.DataAnnotations.Schema;

namespace Magik_Johnson.Models;

[Table("Artists")]
public class Artist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<ArtistCard> ArtistsCards { get; set; } = [];
}
