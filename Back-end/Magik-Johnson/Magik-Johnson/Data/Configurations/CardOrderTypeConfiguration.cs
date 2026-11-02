using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class CardOrderTypeConfiguration : IEntityTypeConfiguration<CardOrder>
{
    public void Configure(EntityTypeBuilder<CardOrder> builder)
    {
        builder.ToTable("CardOrders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.IsFoil).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.LanguageId).IsRequired(false);

        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Quantity_HigherThanZero", "[Quantity] > 0");
        });

        builder.HasOne(x => x.Card).WithMany(c => c.CardOrders).HasForeignKey(c => c.CardId);
        builder.HasOne(x => x.Order).WithMany(x => x.CardOrders).HasForeignKey(x => x.OrderId);
        builder
            .HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
