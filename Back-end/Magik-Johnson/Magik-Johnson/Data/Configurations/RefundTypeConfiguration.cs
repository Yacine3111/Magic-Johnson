using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class RefundTypeConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasOne(r => r.Order).WithMany(o => o.Refunds).HasForeignKey(r => r.OrderId);
        builder.Property(r => r.TotalAmount).HasPrecision(18, 2);
    }
}
