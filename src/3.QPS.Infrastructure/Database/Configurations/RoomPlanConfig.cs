using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database.Configurations;

public class RoomPlanConfig : IEntityTypeConfiguration<RoomPlan>
{
    public void Configure(EntityTypeBuilder<RoomPlan> builder)
    {
        builder.HasKey(rp => rp.Id);

        builder.HasOne(rp => rp.Room)
            .WithMany(r => r.RoomPlans)
            .HasForeignKey(rp => rp.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Plan)
            .WithMany()
            .HasForeignKey(rp => rp.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(rp => rp.MerchantId);
    }
}