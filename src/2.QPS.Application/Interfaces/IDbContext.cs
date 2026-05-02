using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities;

namespace QPS.Application.Interfaces;

public interface IDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Order> Orders { get; }
    DbSet<Merchant> Merchants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Shop> Shops { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Plan> Plans { get; }
    DbSet<RoomImage> RoomImages { get; }
    DbSet<RoomTag> RoomTags { get; }
    DbSet<RoomPlan> RoomPlans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}