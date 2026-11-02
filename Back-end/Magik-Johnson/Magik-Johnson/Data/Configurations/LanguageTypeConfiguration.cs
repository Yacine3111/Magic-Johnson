using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class LanguageTypeConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).IsRequired().HasDefaultValueSql("NEWID()");
        builder.Property(l => l.CardId).IsRequired();
        builder.Property(l => l.ScryfallId).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();

        builder
            .HasOne(l => l.Card)
            .WithMany(c => c.Languages)
            .HasForeignKey(l => l.CardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
