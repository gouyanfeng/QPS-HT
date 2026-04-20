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
        builder.Property(m => m.PhoneNumber).IsRequired().HasMaxLength(20);

        // 配置 StoreSettings 作为值对象
        builder.OwnsOne(m => m.StoreSettings, ss =>
        {
            ss.Property(s => s.PowerOffDelayMinutes).IsRequired();
            ss.Property(s => s.OpeningTime).IsRequired();
            ss.Property(s => s.ClosingTime).IsRequired();
        });
    }
}