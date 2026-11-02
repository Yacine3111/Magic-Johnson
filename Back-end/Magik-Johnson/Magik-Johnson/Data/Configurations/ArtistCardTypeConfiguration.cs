using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class ArtistCardTypeConfiguration : IEntityTypeConfiguration<ArtistCard>
{
    public void Configure(EntityTypeBuilder<ArtistCard> builder)
    {
        builder.ToTable("ArtistCard");
        builder.HasKey(ct => new { ct.CardId, ct.ArtistId });

        builder
            .HasOne(ac => ac.Artist)
            .WithMany(a => a.ArtistsCards)
            .HasForeignKey(ac => ac.ArtistId);

        builder.HasOne(ac => ac.Card).WithMany(c => c.ArtistsCards).HasForeignKey(ac => ac.CardId);
    }
}
