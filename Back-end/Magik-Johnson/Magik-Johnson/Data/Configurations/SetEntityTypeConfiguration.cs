using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class SetEntityTypeConfiguration : IEntityTypeConfiguration<Set>
{
    public void Configure(EntityTypeBuilder<Set> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).IsRequired().HasDefaultValueSql("NEWID()");
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SetCode).HasMaxLength(15).IsRequired();
    }
}
