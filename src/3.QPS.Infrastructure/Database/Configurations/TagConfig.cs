using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database.Configurations;

public class TagConfig : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TagName).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Category).HasMaxLength(50).HasDefaultValue("");

        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(t => t.MerchantId);
    }
}