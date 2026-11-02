using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class DiscountTypeConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).IsRequired().HasDefaultValueSql("NEWID()");
        builder.Property(d => d.Title).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Rate).IsRequired();
        builder.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_Discount_Rate_Range", "[Rate] BETWEEN 1 AND 100");
        });
        builder.Property(d => d.StartDate).IsRequired();
        builder.Property(d => d.EndDate).IsRequired();
        builder.Property(d => d.Status).IsRequired().HasDefaultValue("En Attente");
    }
}
