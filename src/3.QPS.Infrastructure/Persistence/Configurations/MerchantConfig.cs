using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Aggregates.MerchantAggregate;

namespace QPS.Infrastructure.Persistence.Configurations;

public class MerchantConfig : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Phone).IsRequired().HasMaxLength(20);
        builder.Property(m => m.ExpiryDate);
        builder.Property(m => m.IsActive).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();
    }
}