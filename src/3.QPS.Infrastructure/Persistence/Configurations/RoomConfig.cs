using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Aggregates.RoomAggregate;

namespace QPS.Infrastructure.Persistence.Configurations;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.DeviceSn).IsRequired().HasMaxLength(100);
        builder.Property(r => r.MqttTopic).IsRequired().HasMaxLength(200);

        // 配置外键关系
        builder.HasOne<Domain.Aggregates.MerchantAggregate.Merchant>()
            .WithMany()
            .HasForeignKey(r => r.MerchantId);

        builder.HasOne<Domain.Aggregates.MerchantAggregate.Shop>()
            .WithMany()
            .HasForeignKey(r => r.ShopId);
    }
}