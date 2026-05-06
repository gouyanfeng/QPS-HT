using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        builder.Property(r => r.Rating).HasColumnType("decimal(3,2)").HasDefaultValue(0);
        builder.Property(r => r.RatingCount).IsRequired().HasDefaultValue(0);

        builder.HasOne(r => r.Shop)
            .WithMany(s => s.Rooms)
            .HasForeignKey(r => r.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(r => r.MerchantId);
    }
}