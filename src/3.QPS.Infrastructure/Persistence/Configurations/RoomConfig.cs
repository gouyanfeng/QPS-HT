using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Aggregates.RoomAggregate;

namespace QPS.Infrastructure.Persistence.Configurations;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RoomNumber).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.MerchantId).IsRequired();

        builder.OwnsOne(r => r.DeviceConfig, dc =>
        {
            dc.Property(d => d.MqttTopic).IsRequired().HasMaxLength(255);
            dc.Property(d => d.PowerOnCommand).IsRequired().HasMaxLength(255);
            dc.Property(d => d.PowerOffCommand).IsRequired().HasMaxLength(255);
        });
    }
}