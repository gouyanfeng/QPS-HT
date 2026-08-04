using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Common;
using QPS.Domain.Entities.System;
using QPS.Domain.Entities.Crm;
using System.Linq.Expressions;

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
    public DbSet<SystemRegion> SystemRegions { get; set; }
    public DbSet<SystemChinaRegion> SystemChinaRegions { get; set; }
    public DbSet<SystemErrorLog> SystemErrorLogs { get; set; }
    public DbSet<SystemOperationLog> SystemOperationLogs { get; set; }

    // CRM 模块
    public DbSet<CrmHerbBaseSubject> CrmHerbBaseSubjects { get; set; }
    public DbSet<CrmHerbBase> CrmHerbBases { get; set; }
    public DbSet<CrmContact> CrmContacts { get; set; }
    public DbSet<CrmFollowRecord> CrmFollowRecords { get; set; }
    public DbSet<CrmBusinessEntityAttribute> CrmBusinessEntityAttributes { get; set; }
    public DbSet<CrmTransferRecord> CrmTransferRecords { get; set; }
    public DbSet<CrmVendor> CrmVendors { get; set; }
    public DbSet<CrmVendorPurchasePlan> CrmVendorPurchasePlans { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<CrmHerbBase>(entity =>
        {
            entity.Ignore(herbBase => herbBase.HerbBaseName);
            entity.Property(herbBase => herbBase.BaseName).HasMaxLength(200);
            entity.Property(herbBase => herbBase.SubjectName).HasMaxLength(200);
            entity.Property(herbBase => herbBase.Scale).HasPrecision(18, 2);
            entity.Property(herbBase => herbBase.Lat).HasPrecision(10, 6);
            entity.Property(herbBase => herbBase.Lng).HasPrecision(10, 6);
            entity.HasOne(herbBase => herbBase.HerbBaseSubject)
                .WithMany(subject => subject.HerbBases)
                .HasForeignKey(herbBase => herbBase.HerbBaseSubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CrmHerbBaseSubject>(entity =>
        {
            entity.Property(subject => subject.SubjectName).HasMaxLength(200);
            entity.Property(subject => subject.NormalizedSubjectName).HasMaxLength(500);
            entity.Property(subject => subject.DisplayName).HasMaxLength(200);
            entity.Property(subject => subject.SubjectType).HasMaxLength(32);
            entity.Property(subject => subject.Status).HasMaxLength(32);
            entity.Property(subject => subject.Grade).HasMaxLength(32);
            entity.Property(subject => subject.PrimaryContactName).HasMaxLength(200);
            entity.Property(subject => subject.PrimaryContactPhone).HasMaxLength(100);
            entity.HasIndex(subject => subject.NormalizedSubjectName).IsUnique();
            entity.HasIndex(subject => subject.DisplayName);
            entity.HasIndex(subject => new { subject.SubjectType, subject.IsDeleted });
        });

        modelBuilder.Entity<CrmFollowRecord>(entity =>
        {
            entity.HasOne(record => record.HerbBaseSubject)
                .WithMany(subject => subject.FollowRecords)
                .HasForeignKey(record => record.HerbBaseSubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(record => record.HerbBase)
                .WithMany()
                .HasForeignKey(record => record.HerbBaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CrmBusinessEntityAttribute>(entity =>
        {
            entity.HasIndex(attribute => new
            {
                attribute.EntityType,
                attribute.EntityId,
                attribute.AttributeCode
            });

            entity.HasIndex(attribute => new
            {
                attribute.EntityType,
                attribute.EntityId,
                attribute.AttributeCode,
                attribute.AttributeValue
            });
        });

        modelBuilder.Entity<CrmContact>(entity =>
        {
            entity.Property(contact => contact.EntityType).HasMaxLength(64);
            entity.Property(contact => contact.ContactName).HasMaxLength(200);
            entity.Property(contact => contact.Phone).HasMaxLength(100);
            entity.Property(contact => contact.PhoneType).HasMaxLength(50);
            entity.Property(contact => contact.Wechat).HasMaxLength(100);
            entity.Property(contact => contact.RoleName).HasMaxLength(100);
            entity.Property(contact => contact.Status).HasMaxLength(50);
            entity.HasIndex(contact => new { contact.EntityType, contact.EntityId });
            entity.HasIndex(contact => new { contact.EntityType, contact.EntityId, contact.Phone });
        });

        modelBuilder.Entity<CrmVendor>(entity =>
        {
            entity.Property(vendor => vendor.VendorName).HasMaxLength(200);
            entity.Property(vendor => vendor.NormalizedVendorName).HasMaxLength(200);
            entity.Property(vendor => vendor.PriorityLevel).HasMaxLength(20);
            entity.HasIndex(vendor => vendor.NormalizedVendorName).IsUnique();
            entity.HasIndex(vendor => vendor.PriorityLevel);
            entity.HasIndex(vendor => vendor.LatestPurchaseTime);
        });

        modelBuilder.Entity<CrmVendorPurchasePlan>(entity =>
        {
            entity.Property(plan => plan.PurchasePlanName).HasMaxLength(500);
            entity.Property(plan => plan.PageUrl).HasMaxLength(500);
            entity.HasIndex(plan => plan.VendorId);
            entity.HasIndex(plan => plan.PurchaseTime);
            entity.HasIndex(plan => plan.PageUrl).IsUnique();
        });

        modelBuilder.Entity<CrmTransferRecord>(entity =>
        {
            entity.Property(record => record.EntityType).HasMaxLength(64);
            entity.HasIndex(record => new { record.EntityType, record.EntityId, record.CreatedAt });
        });

        modelBuilder.Entity<SystemChinaRegion>(entity =>
        {
            entity.HasIndex(region => region.Code);
            entity.HasIndex(region => new { region.Level, region.ParentCode });
            entity.Property(region => region.Code).HasMaxLength(20);
            entity.Property(region => region.Name).HasMaxLength(100);
            entity.Property(region => region.FullName).HasMaxLength(200);
            entity.Property(region => region.ParentCode).HasMaxLength(20);
            entity.Property(region => region.ProvinceCode).HasMaxLength(20);
            entity.Property(region => region.CityCode).HasMaxLength(20);
        });

        modelBuilder.Entity<SystemOperationLog>(entity =>
        {
            entity.Property(log => log.ActionType).HasMaxLength(64);
            entity.Property(log => log.EntityType).HasMaxLength(128);
            entity.Property(log => log.EntityId).HasMaxLength(64);
            entity.Property(log => log.OperatorName).HasMaxLength(100);
            entity.Property(log => log.RequestPath).HasMaxLength(500);
            entity.Property(log => log.IpAddress).HasMaxLength(64);
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => new { log.EntityType, log.EntityId });
            entity.HasIndex(log => log.ActionType);
        });

        ApplySoftDeleteQueryFilters(modelBuilder);
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

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(entityType => typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var isDeleted = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Equal(isDeleted, Expression.Constant(false)), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}




