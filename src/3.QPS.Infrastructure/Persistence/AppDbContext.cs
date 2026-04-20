using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.RoomAggregate;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.MerchantAggregate;
using QPS.Domain.Aggregates.UserAggregate;
using QPS.Domain.Aggregates.PricingAggregate;
using QPS.Domain.Aggregates.OperationAggregate;

namespace QPS.Infrastructure.Persistence;

public class AppDbContext : DbContext, IDbContext
{
    // 租户与门店
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Shop> Shops { get; set; }

    // 权限(RBAC)
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    // 空间展示
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomImage> RoomImages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<RoomTagMapping> RoomTagMappings { get; set; }

    // 计费优惠
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CustomerCoupon> CustomerCoupons { get; set; }
    public DbSet<Discount> Discounts { get; set; }

    // 交易订单
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    // 运维审计
    public DbSet<DeviceOperationLog> DeviceOperationLogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}