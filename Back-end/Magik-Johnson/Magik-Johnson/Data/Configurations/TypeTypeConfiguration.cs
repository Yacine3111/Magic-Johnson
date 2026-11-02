using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Type = Magik_Johnson.Models.Type;

namespace Magik_Johnson.Data.Configurations;

public class TypeTypeConfiguration : IEntityTypeConfiguration<Type>
{
    public void Configure(EntityTypeBuilder<Type> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).IsRequired().HasDefaultValueSql("NEWID()");
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
    }
}
