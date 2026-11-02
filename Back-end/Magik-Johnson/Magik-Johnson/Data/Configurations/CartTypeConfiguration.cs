using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class CartTypeConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.IdUser).HasMaxLength(450).IsRequired();

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.IdUser);
    }
}
