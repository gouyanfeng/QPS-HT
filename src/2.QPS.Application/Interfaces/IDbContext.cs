using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities.System;

namespace QPS.Application.Interfaces;

public interface IDbContext
{
    DbSet<SystemUser> SystemUsers { get; }
    DbSet<SystemRole> SystemRoles { get; }
    DbSet<SystemPermission> SystemPermissions { get; }
    DbSet<SystemUserRole> SystemUserRoles { get; }
    DbSet<SystemRolePermission> SystemRolePermissions { get; }
    DbSet<SystemDataDictionary> SystemDataDictionaries { get; }
    DbSet<SystemErrorLog> SystemErrorLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
