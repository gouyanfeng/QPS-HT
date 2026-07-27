using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QPS.Application.Interfaces;
using QPS.Domain.Common;
using QPS.Domain.Entities.System;
using QPS.Domain.Entities.Crm;
using System.Text.Json;

namespace QPS.Infrastructure.Database;

public class AppDbContext : DbContext, IDbContext
{
    private static readonly HashSet<string> IgnoredOperationLogFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(BaseEntity.Id),
        nameof(BaseEntity.CreatedAt),
        nameof(BaseEntity.CreatedBy),
        nameof(BaseEntity.UpdatedAt),
        nameof(BaseEntity.UpdatedBy),
        nameof(BaseEntity.IsDeleted),
        "Password",
        "PasswordHash",
        "Token",
        "RefreshToken"
    };

    private static readonly HashSet<string> StatusFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Status",
        "State",
        "Enabled",
        "IsEnabled",
        "IsActive"
    };

    private static readonly HashSet<string> OwnerFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "OwnerUserId",
        "AssigneeId",
        "ResponsibleUserId",
        "ManagerUserId"
    };

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
        var operationLogs = CollectOperationLogs();
        SystemOperationLogs.AddRange(operationLogs);
        SetAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var operationLogs = CollectOperationLogs();
        await SystemOperationLogs.AddRangeAsync(operationLogs, cancellationToken);
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<SystemOperationLog> CollectOperationLogs()
    {
        ChangeTracker.DetectChanges();

        return ChangeTracker.Entries()
            .Where(entry => entry.Entity is BaseEntity)
            .Where(entry => entry.Entity is not SystemOperationLog)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CreateOperationLog)
            .Where(log => log is not null)
            .Cast<SystemOperationLog>()
            .ToList();
    }

    private SystemOperationLog? CreateOperationLog(EntityEntry entry)
    {
        var changeMap = BuildChangeMap(entry);
        if (changeMap.Count == 0)
        {
            return null;
        }

        var entity = (BaseEntity)entry.Entity;
        var actionType = ResolveActionType(entry.State, changeMap.Keys);
        var changeJson = JsonSerializer.Serialize(changeMap);

        return SystemOperationLog.Create(
            entry.Metadata.ClrType.Name,
            entity.Id.ToString(),
            actionType,
            changeJson,
            _currentUserService.UserId ?? string.Empty,
            _currentUserService.Username ?? "System",
            _currentUserService.RequestPath ?? string.Empty,
            _currentUserService.IpAddress ?? string.Empty,
            _currentUserService.UserAgent ?? string.Empty);
    }

    private static Dictionary<string, OperationLogChange> BuildChangeMap(EntityEntry entry)
    {
        var changes = new Dictionary<string, OperationLogChange>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;
            if (IgnoredOperationLogFields.Contains(propertyName))
            {
                continue;
            }

            if (entry.State == EntityState.Modified && !property.IsModified)
            {
                continue;
            }

            var oldValue = entry.State == EntityState.Added ? null : property.OriginalValue;
            var newValue = entry.State == EntityState.Deleted ? null : property.CurrentValue;

            if (entry.State == EntityState.Modified && Equals(oldValue, newValue))
            {
                continue;
            }

            changes[propertyName] = new OperationLogChange(oldValue, newValue);
        }

        return changes;
    }

    private static string ResolveActionType(EntityState state, IEnumerable<string> changedFields)
    {
        if (state == EntityState.Added)
        {
            return "Create";
        }

        if (state == EntityState.Deleted)
        {
            return "Delete";
        }

        var fields = changedFields.ToList();
        if (fields.Count > 0 && fields.All(StatusFields.Contains))
        {
            return "StatusChange";
        }

        if (fields.Count > 0 && fields.All(OwnerFields.Contains))
        {
            return "AssignOwner";
        }

        return "Update";
    }

    private sealed record OperationLogChange(
        [property: System.Text.Json.Serialization.JsonPropertyName("old")] object? Old,
        [property: System.Text.Json.Serialization.JsonPropertyName("new")] object? New);

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
