using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database.Configurations;

public class RoomImageConfig : IEntityTypeConfiguration<RoomImage>
{
    public void Configure(EntityTypeBuilder<RoomImage> builder)
    {
        builder.HasKey(ri => ri.Id);
        builder.Property(ri => ri.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(ri => ri.IsMain).IsRequired();
        builder.Property(ri => ri.SortOrder).IsRequired();

        builder.HasOne(ri => ri.Room)
            .WithMany(r => r.RoomImages)
            .HasForeignKey(ri => ri.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(ri => ri.MerchantId);
    }
}