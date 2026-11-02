using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class CardCartTypeConfiguration : IEntityTypeConfiguration<CardCart>
{
    public void Configure(EntityTypeBuilder<CardCart> builder)
    {
        builder.ToTable("CardCart");
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Quantity).IsRequired();

        builder.HasOne(ct => ct.Card).WithMany(c => c.CardCarts).HasForeignKey(c => c.CardId);
        builder.HasOne(ct => ct.Cart).WithMany(c => c.CardCarts).HasForeignKey(c => c.CartId);
        builder
            .HasOne(ct => ct.Language)
            .WithMany(c => c.CardCarts)
            .HasForeignKey(c => c.LanguageId);
    }
}
