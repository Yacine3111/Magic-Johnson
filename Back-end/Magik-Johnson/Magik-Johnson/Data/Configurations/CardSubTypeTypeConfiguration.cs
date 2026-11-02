using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class CardSubTypeTypeConfiguration : IEntityTypeConfiguration<CardSubType>
{
    public void Configure(EntityTypeBuilder<CardSubType> builder)
    {
        builder.ToTable("CardSubType");
        builder.HasKey(ct => new { ct.CardId, ct.SubTypeId });

        builder.HasOne(ct => ct.Card).WithMany(c => c.CardSubTypes).HasForeignKey(ct => ct.CardId);

        builder
            .HasOne(ct => ct.SubType)
            .WithMany(t => t.CardSubTypes)
            .HasForeignKey(ct => ct.SubTypeId);
    }
}
