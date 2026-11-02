using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class OrderTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.PaymentIntentId).IsRequired();
        builder.Property(x => x.SubTotal).IsRequired().HasPrecision(9, 2);
        builder.Property(x => x.TaxeAmount).IsRequired().HasPrecision(9, 2);

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId);
        builder
            .HasOne(o => o.OrderStatus)
            .WithMany()
            .HasForeignKey(o => o.OrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
