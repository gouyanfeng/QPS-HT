using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Entities;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database.Configurations;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.IsEnabled).IsRequired();



        // 配置外键关系
        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(r => r.MerchantId);

        builder.HasOne<Shop>()
            .WithMany()
            .HasForeignKey(r => r.ShopId);
    }
}