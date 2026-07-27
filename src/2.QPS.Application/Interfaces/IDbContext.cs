using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities.System;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Interfaces;

public interface IDbContext
{
    DbSet<SystemUser> SystemUsers { get; }
    DbSet<SystemRole> SystemRoles { get; }
    DbSet<SystemPermission> SystemPermissions { get; }
    DbSet<SystemUserRole> SystemUserRoles { get; }
    DbSet<SystemRolePermission> SystemRolePermissions { get; }
    DbSet<SystemDataDictionary> SystemDataDictionaries { get; }
    DbSet<SystemRegion> SystemRegions { get; }
    DbSet<SystemChinaRegion> SystemChinaRegions { get; }
    DbSet<BusinessEntityAttribute> BusinessEntityAttributes { get; }
    DbSet<SystemErrorLog> SystemErrorLogs { get; }
    DbSet<SystemOperationLog> SystemOperationLogs { get; }

    // CRM 模块
    DbSet<CrmCustomer> CrmCustomers { get; }
    DbSet<CrmContact> CrmContacts { get; }
    DbSet<CrmFollowRecord> CrmFollowRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
