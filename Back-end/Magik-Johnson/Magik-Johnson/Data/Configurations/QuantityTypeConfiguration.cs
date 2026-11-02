using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class QuantityTypeConfiguration : IEntityTypeConfiguration<Quantity>
{
    public void Configure(EntityTypeBuilder<Quantity> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.NonFoilQuantity).IsRequired();
        builder.Property(q => q.FoilQuantity).IsRequired();

        builder
            .HasOne(q => q.Language)
            .WithMany(l => l.Quantities)
            .HasForeignKey(q => q.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(q => q.Card)
            .WithMany(c => c.Quantities)
            .HasForeignKey(q => q.CardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
