using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Aggregates.OrderAggregate;

namespace QPS.Infrastructure.Persistence.Configurations;

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNo).IsRequired().HasMaxLength(50);
        builder.Property(o => o.MerchantId).IsRequired();
        builder.Property(o => o.ShopId).IsRequired();
        builder.Property(o => o.RoomId).IsRequired();
        builder.Property(o => o.CustomerId).IsRequired(false);
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.OriginAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.ActualAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.StartTime).IsRequired();
        builder.Property(o => o.EndTime).IsRequired(false);
        builder.Property(o => o.PaymentMethod).HasMaxLength(50).IsRequired(false);
        builder.Property(o => o.PaidAt).IsRequired(false);

        // 索引
        builder.HasIndex(o => o.OrderNo).IsUnique();

     
    }
}