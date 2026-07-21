using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Common;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;
using QPS.Domain.Entities.System;

namespace QPS.Infrastructure.Database;

public class AppDbContext : DbContext, IDbContext
{
    private readonly ICurrentUserService _currentUserService;

    // 租户与门店
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Shop> Shops { get; set; }

    // 权限(RBAC)
    public DbSet<SystemUser> SystemUsers { get; set; }
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<SystemPermission> SystemPermissions { get; set; }
    public DbSet<SystemUserRole> SystemUserRoles { get; set; }
    public DbSet<SystemRolePermission> SystemRolePermissions { get; set; }
    public DbSet<SystemDataDictionary> SystemDataDictionaries { get; set; }
    public DbSet<SystemErrorLog> SystemErrorLogs { get; set; }

    // 空间展示
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomImage> RoomImages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<RoomTag> RoomTags { get; set; }
    public DbSet<RoomPlan> RoomPlans { get; set; }

    // 计费优惠
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CustomerCoupon> CustomerCoupons { get; set; }
    public DbSet<Discount> Discounts { get; set; }

    // 交易订单
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    // 运维审计
    public DbSet<DeviceOperationLog> DeviceOperationLogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        var currentUser = _currentUserService.Username ?? "System";
        var now = DateTime.UtcNow;

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.CreatedBy = currentUser;
            }

            entity.UpdatedAt = now;
            entity.UpdatedBy = currentUser;
        }
    }
}