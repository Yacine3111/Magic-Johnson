using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class CardTypeConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).IsRequired();
        builder.Property(c => c.SetId).IsRequired();
        builder.Property(c => c.ScryfallId).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.Name);
        builder.Property(c => c.SortableName).HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.SortableName);
        builder.Property(c => c.FrenchName).HasMaxLength(200);
        builder.HasIndex(c => c.FrenchName);
        builder.Property(c => c.ManaCost).HasMaxLength(100);
        builder.Property(c => c.Text).HasMaxLength(1000);
        builder.HasIndex(c => c.Text);
        builder.Property(c => c.Power).HasMaxLength(5);
        builder.Property(c => c.Toughness).HasMaxLength(5);
        builder.Property(c => c.Rarity).HasMaxLength(15).IsRequired();
        builder.Property(c => c.Number).HasMaxLength(10).IsRequired();
        builder.Property(c => c.HasFoil).IsRequired();
        builder.Property(c => c.HasNonFoil).IsRequired();
        builder.Property(c => c.Price).HasPrecision(9, 2).IsRequired();
        builder.Property(c => c.FoilPrice).HasPrecision(9, 2).IsRequired();
        builder.Property(c => c.OnlineOnly).IsRequired();

        builder.HasOne<Set>().WithMany().HasForeignKey(c => c.SetId);
        builder.HasOne<Discount>().WithMany().HasForeignKey(c => c.DiscountId);
    }
}
