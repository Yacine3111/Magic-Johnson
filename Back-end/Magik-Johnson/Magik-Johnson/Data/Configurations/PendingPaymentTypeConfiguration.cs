using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class PendingPaymentTypeConfiguration : IEntityTypeConfiguration<PendingPayment>
{
    public void Configure(EntityTypeBuilder<PendingPayment> builder)
    {
        builder.ToTable("PendingPayments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId).IsRequired().HasMaxLength(450);

        builder.Property(p => p.SessionId).IsRequired().HasMaxLength(255);

        builder.Property(p => p.ShippingAddress).HasMaxLength(1000);

        builder.Property(p => p.TotalAmount).HasPrecision(10, 2);

        // ✅ SQL Server n’a pas de type JSONB : on utilise nvarchar(max)
        builder.Property(p => p.ItemsJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasIndex(p => p.SessionId).IsUnique();
    }
}
