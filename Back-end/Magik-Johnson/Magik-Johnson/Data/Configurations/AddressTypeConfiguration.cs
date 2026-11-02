using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class AddressTypeConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Property(a => a.CountryId).IsRequired();
        builder.Property(a => a.DoorNb).IsRequired();
        builder.Property(a => a.Street).HasMaxLength(200).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.HasOne(a => a.Country).WithMany().HasForeignKey(a => a.CountryId);

        builder.HasOne(a => a.Province).WithMany().HasForeignKey(a => a.ProvinceId);

        builder
            .HasOne(a => a.ApplicationUser)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.ApplicationUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
