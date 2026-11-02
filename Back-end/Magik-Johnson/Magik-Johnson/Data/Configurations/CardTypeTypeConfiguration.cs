using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class CardTypeTypeConfiguration : IEntityTypeConfiguration<CardType>
{
    public void Configure(EntityTypeBuilder<CardType> builder)
    {
        builder.ToTable("CardType");
        builder.HasKey(ct => new { ct.CardId, ct.TypeId });

        builder.HasOne(ct => ct.Card).WithMany(c => c.CardTypes).HasForeignKey(ct => ct.CardId);

        builder.HasOne(ct => ct.Type).WithMany(t => t.CardTypes).HasForeignKey(ct => ct.TypeId);
    }
}
