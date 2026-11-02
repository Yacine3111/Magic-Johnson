using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class SubTypeTypeConfiguration : IEntityTypeConfiguration<SubType>
{
    public void Configure(EntityTypeBuilder<SubType> builder)
    {
        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id).IsRequired().HasDefaultValueSql("NEWID()");
        builder.Property(st => st.Name).HasMaxLength(100).IsRequired();
    }
}
