using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database.Configurations;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.RoomId).IsRequired();
        builder.Property(r => r.CustomerId).IsRequired();
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.Content).HasMaxLength(500);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}