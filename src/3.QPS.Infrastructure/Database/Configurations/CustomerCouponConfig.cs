using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database.Configurations;

public class CustomerCouponConfig : IEntityTypeConfiguration<CustomerCoupon>
{
    public void Configure(EntityTypeBuilder<CustomerCoupon> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CouponId).IsRequired();
        builder.Property(c => c.CustomerId).IsRequired();
        builder.Property(c => c.Status).IsRequired().HasMaxLength(20);

        builder.HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(c => c.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}