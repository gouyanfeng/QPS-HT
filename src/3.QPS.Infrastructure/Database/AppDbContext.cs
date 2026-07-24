using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Common;
using QPS.Domain.Entities.System;

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
