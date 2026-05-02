using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Common;
using QPS.Domain.Entities;

namespace QPS.Infrastructure.Database;

public class AppDbContext : DbContext, IDbContext
{
    private readonly ICurrentUserService _currentUserService;

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
    // 运维审计
    public DbSet<DeviceOperationLog> DeviceOperationLogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // 添加全局查询过滤器，根据当前租户ID过滤数据
        // 使用延迟求值，让MerchantId在每次查询时动态获取
        modelBuilder.Entity<Shop>().HasQueryFilter(s => s.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<User>().HasQueryFilter(u => u.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Role>().HasQueryFilter(r => r.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Room>().HasQueryFilter(r => r.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Tag>().HasQueryFilter(t => t.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Plan>().HasQueryFilter(p => p.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Coupon>().HasQueryFilter(c => c.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Discount>().HasQueryFilter(d => d.MerchantId == _currentUserService.MerchantId);
        modelBuilder.Entity<Order>().HasQueryFilter(o => o.MerchantId == _currentUserService.MerchantId);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        SetMerchantIdForNewEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        SetMerchantIdForNewEntities();
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

    private void SetMerchantIdForNewEntities()
    {
        var merchantId = _currentUserService.MerchantId;
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            var merchantIdProperty = entity.GetType().GetProperty("MerchantId");

            if (merchantIdProperty != null)
            {
                var currentValue = merchantIdProperty.GetValue(entity);
                if (currentValue is Guid guid && guid == Guid.Empty)
                {
                    merchantIdProperty.SetValue(entity, merchantId);
                }
            }
        }
    }
}