using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Aggregates.OrderAggregate;

namespace QPS.Infrastructure.Persistence.Configurations;

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.RoomId).IsRequired();
        builder.Property(o => o.MerchantId).IsRequired();
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.Amount).IsRequired();
        builder.Property(o => o.StartTime).IsRequired();
        builder.Property(o => o.EndTime);

        builder.OwnsOne(o => o.PricingStrategy, ps =>
        {
            ps.Property(p => p.BasePricePerHour).IsRequired();
            ps.Property(p => p.StepPricePerHour).IsRequired();
            ps.Property(p => p.StepHours).IsRequired();
            ps.Property(p => p.DailyCap).IsRequired();
        });
    }
}