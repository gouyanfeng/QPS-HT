using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;
using QPS.Domain.Entities.System;
namespace QPS.Application.Interfaces;

public interface IDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Order> Orders { get; }
    DbSet<Merchant> Merchants { get; }
    DbSet<SystemUser> SystemUsers { get; }
    DbSet<SystemRole> SystemRoles { get; }
    DbSet<SystemPermission> SystemPermissions { get; }
    DbSet<SystemRolePermission> SystemRolePermissions { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Shop> Shops { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Plan> Plans { get; }
    DbSet<RoomImage> RoomImages { get; }
    DbSet<RoomTag> RoomTags { get; }
    DbSet<RoomPlan> RoomPlans { get; }
    DbSet<CustomerCoupon> CustomerCoupons { get; }
    DbSet<Review> Reviews { get; }
    DbSet<SystemDataDictionary> SystemDataDictionaries { get; }
    DbSet<SystemErrorLog> SystemErrorLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}