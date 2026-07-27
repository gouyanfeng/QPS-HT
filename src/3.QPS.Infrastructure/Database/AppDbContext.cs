using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Common;
using QPS.Domain.Entities.System;
using QPS.Domain.Entities.Crm;

namespace QPS.Infrastructure.Database;

public class AppDbContext : DbContext, IDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public DbSet<SystemUser> SystemUsers { get; set; }
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<SystemPermission> SystemPermissions { get; set; }
    public DbSet<SystemUserRole> SystemUserRoles { get; set; }
    public DbSet<SystemRolePermission> SystemRolePermissions { get; set; }
    public DbSet<SystemDataDictionary> SystemDataDictionaries { get; set; }
    public DbSet<SystemErrorLog> SystemErrorLogs { get; set; }
    public DbSet<SystemOperationLog> SystemOperationLogs { get; set; }

    // CRM 模块
    public DbSet<CrmCustomer> CrmCustomers { get; set; }
    public DbSet<CrmContact> CrmContacts { get; set; }
    public DbSet<CrmFollowRecord> CrmFollowRecords { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<CrmCustomer>(entity =>
        {
            entity.Property(customer => customer.Lat).HasPrecision(10, 6);
            entity.Property(customer => customer.Lng).HasPrecision(10, 6);
        });

        modelBuilder.Entity<SystemOperationLog>(entity =>
        {
            entity.HasIndex(log => new { log.EntityType, log.EntityId, log.CreatedAt });
            entity.HasIndex(log => new { log.OperatorUserId, log.CreatedAt });
            entity.HasIndex(log => new { log.ActionType, log.CreatedAt });

            entity.Property(log => log.EntityType).HasMaxLength(100);
            entity.Property(log => log.EntityId).HasMaxLength(64);
            entity.Property(log => log.ActionType).HasMaxLength(50);
            entity.Property(log => log.OperatorUserId).HasMaxLength(64);
            entity.Property(log => log.OperatorName).HasMaxLength(100);
            entity.Property(log => log.RequestPath).HasMaxLength(300);
            entity.Property(log => log.IpAddress).HasMaxLength(64);
            entity.Property(log => log.UserAgent).HasMaxLength(500);
        });
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
