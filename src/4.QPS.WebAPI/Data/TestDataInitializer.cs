using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities.System;
using QPS.Domain.Entities.Crm;
using QPS.Infrastructure.Database;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        var roles = InitializeRoles(dbContext);
        InitializeUsers(dbContext, roles);
        var permissions = InitializePermissions(dbContext, roles);
        EnsureCrmHerbBaseLegacyTables(dbContext);
        EnsureCrmBusinessEntityAttributesTable(dbContext);
        EnsureCrmTransferRecordsTable(dbContext);
        EnsureCrmContactsEntityColumns(dbContext);
        EnsureCrmVendorsTable(dbContext);
        EnsureCrmVendorPurchasePlansTable(dbContext);
        EnsureCrmHerbBasesBaseNameColumn(dbContext);
        EnsureCrmHerbBasesSourceIdColumn(dbContext);
        EnsureCrmHerbBasesSubjectNameColumn(dbContext);
        EnsureCrmHerbBasesScaleColumn(dbContext);
        InitializeDataDictionaries(dbContext);
        InitializeCrm(dbContext, permissions);
        NormalizeCrmBusinessValues(dbContext);
        NormalizeCrmMainProducts(dbContext);
        EnsureDefaultCrmOwner(dbContext);
    }

    private static void EnsureCrmHerbBaseLegacyTables(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NULL
                AND OBJECT_ID(N'[CrmCustomers]', N'U') IS NOT NULL
            BEGIN
                EXEC sp_rename N'dbo.CrmCustomers', N'CrmHerbBases';
            END;

            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'BaseName') IS NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'CustomerName') IS NOT NULL
            BEGIN
                EXEC sp_rename N'dbo.CrmHerbBases.CustomerName', N'BaseName', N'COLUMN';
            END;

            """);
    }

    private static List<SystemRole> InitializeRoles(AppDbContext dbContext)
    {
        var existingRoles = dbContext.SystemRoles.ToList();

        if (existingRoles.Any())
        {
            return existingRoles;
        }

        var roles = new List<SystemRole>
        {
            new("管理员", "admin"),
            new("用户", "user")
        };

        dbContext.SystemRoles.AddRange(roles);
        dbContext.SaveChanges();

        return roles;
    }

    private static void InitializeUsers(AppDbContext dbContext, List<SystemRole> roles)
    {
        if (dbContext.SystemUsers.Any())
        {
            return;
        }

        var adminRole = roles.First(r => r.Code == "admin");
        var userRole = roles.First(r => r.Code == "user");

        var users = new List<SystemUser>
        {
            SystemUser.Create("admin", "123456", "系统管理员", adminRole.Id),
            SystemUser.Create("user", "123456", "普通用户", userRole.Id)
        };

        dbContext.SystemUsers.AddRange(users);
        dbContext.SaveChanges();

        var userRoles = new List<SystemUserRole>
        {
            new(users[0].Id, adminRole.Id),
            new(users[1].Id, userRole.Id)
        };

        dbContext.SystemUserRoles.AddRange(userRoles);
        dbContext.SaveChanges();
    }

    private static List<SystemPermission> InitializePermissions(AppDbContext dbContext, List<SystemRole> roles)
    {
        NormalizeSystemPermissions(dbContext);
        var permissions = EnsureStandardPermissions(dbContext, roles);
        return permissions;
    }

    private static List<SystemPermission> EnsureStandardPermissions(AppDbContext dbContext, List<SystemRole> roles)
    {
        var permissions = dbContext.SystemPermissions.ToList();
        var root = EnsurePermission(dbContext, permissions, "权限管理", "ROOT", null);
        var home = EnsurePermission(dbContext, permissions, "首页", "HOME", root.Id);
        var system = EnsurePermission(dbContext, permissions, "系统设置", "SYSTEM", root.Id);
        var role = EnsurePermission(dbContext, permissions, "角色设置", "SYSTEM_ROLE", system.Id);
        EnsurePermission(dbContext, permissions, "新增", "SYSTEM_ROLE_ADD", role.Id);
        EnsurePermission(dbContext, permissions, "编辑", "SYSTEM_ROLE_EDIT", role.Id);
        EnsurePermission(dbContext, permissions, "删除", "SYSTEM_ROLE_DELETE", role.Id);
        var permission = EnsurePermission(dbContext, permissions, "权限设置", "SYSTEM_PERMISSION", system.Id);
        EnsurePermission(dbContext, permissions, "新增", "SYSTEM_PERMISSION_ADD", permission.Id);
        EnsurePermission(dbContext, permissions, "编辑", "SYSTEM_PERMISSION_EDIT", permission.Id);
        EnsurePermission(dbContext, permissions, "删除", "SYSTEM_PERMISSION_DELETE", permission.Id);
        var users = EnsurePermission(dbContext, permissions, "用户管理", "SYSTEM_USER", system.Id);
        EnsurePermission(dbContext, permissions, "新增", "SYSTEM_USER_ADD", users.Id);
        EnsurePermission(dbContext, permissions, "编辑", "SYSTEM_USER_EDIT", users.Id);
        var dataDictionary = EnsurePermission(dbContext, permissions, "数据字典", "SYSTEM_DATA_DICTIONARY", system.Id);
        EnsurePermission(dbContext, permissions, "新增", "SYSTEM_DATA_DICTIONARY_ADD", dataDictionary.Id);
        EnsurePermission(dbContext, permissions, "编辑", "SYSTEM_DATA_DICTIONARY_EDIT", dataDictionary.Id);
        EnsurePermission(dbContext, permissions, "删除", "SYSTEM_DATA_DICTIONARY_DELETE", dataDictionary.Id);
        var region = EnsurePermission(dbContext, permissions, "地址区域维护", "SYSTEM_REGION", system.Id);
        EnsurePermission(dbContext, permissions, "新增", "SYSTEM_REGION_ADD", region.Id);
        EnsurePermission(dbContext, permissions, "编辑", "SYSTEM_REGION_EDIT", region.Id);
        EnsurePermission(dbContext, permissions, "删除", "SYSTEM_REGION_DELETE", region.Id);
        EnsurePermission(dbContext, permissions, "操作日志", "SYSTEM_OPERATION_LOG", system.Id);
        var crmHerbBase = EnsurePermission(dbContext, permissions, "基地管理", "CRM_HERB_BASE", root.Id);
        EnsurePermission(dbContext, permissions, "新增", "CRM_HERB_BASE_ADD", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "编辑", "CRM_HERB_BASE_EDIT", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "删除", "CRM_HERB_BASE_DELETE", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "分配", "CRM_HERB_BASE_ASSIGN", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "记录沟通", "CRM_HERB_BASE_FOLLOW", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "新增联系人", "CRM_HERB_BASE_CONTACT_ADD", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "编辑联系人", "CRM_HERB_BASE_CONTACT_EDIT", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "设置主联系人", "CRM_HERB_BASE_CONTACT_PRIMARY", crmHerbBase.Id);
        EnsurePermission(dbContext, permissions, "标记状态", "CRM_HERB_BASE_STATUS", crmHerbBase.Id);
        var crmVendor = EnsurePermission(dbContext, permissions, "厂商管理", "CRM_VENDOR", root.Id);
        EnsurePermission(dbContext, permissions, "新增", "CRM_VENDOR_ADD", crmVendor.Id);
        EnsurePermission(dbContext, permissions, "编辑", "CRM_VENDOR_EDIT", crmVendor.Id);
        EnsurePermission(dbContext, permissions, "删除", "CRM_VENDOR_DELETE", crmVendor.Id);
        EnsurePermission(dbContext, permissions, "分配", "CRM_VENDOR_ASSIGN", crmVendor.Id);

        dbContext.SaveChanges();
        permissions = dbContext.SystemPermissions.ToList();

        var adminRole = roles.FirstOrDefault(r => r.Code == "admin");
        if (adminRole is not null)
        {
            foreach (var item in permissions.Where(p => p.Code != "ROOT"))
            {
                var exists = dbContext.SystemRolePermissions
                    .Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == item.Id);

                if (!exists)
                {
                    dbContext.SystemRolePermissions.Add(new SystemRolePermission(adminRole.Id, item.Id));
                }
            }
        }

        var userRole = roles.FirstOrDefault(r => r.Code == "user");
        if (userRole is not null)
        {
            var exists = dbContext.SystemRolePermissions.Any(rp => rp.RoleId == userRole.Id && rp.PermissionId == home.Id);
            if (!exists)
            {
                dbContext.SystemRolePermissions.Add(new SystemRolePermission(userRole.Id, home.Id));
            }

            RemoveDefaultUserButtonPermissions(dbContext, userRole.Id);
        }

        dbContext.SaveChanges();
        RemoveRetiredPermissions(dbContext);
        return permissions;
    }

    private static void RemoveRetiredPermissions(AppDbContext dbContext)
    {
        var retiredPermissionCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CRM_HERB_BASE_CONTACT_INVALID"
        };

        var retiredPermissions = dbContext.SystemPermissions
            .Where(permission => retiredPermissionCodes.Contains(permission.Code))
            .ToList();
        if (retiredPermissions.Count == 0)
        {
            return;
        }

        var retiredPermissionIds = retiredPermissions.Select(permission => permission.Id).ToHashSet();
        var rolePermissions = dbContext.SystemRolePermissions
            .Where(rolePermission => retiredPermissionIds.Contains(rolePermission.PermissionId))
            .ToList();

        dbContext.SystemRolePermissions.RemoveRange(rolePermissions);
        dbContext.SystemPermissions.RemoveRange(retiredPermissions);
        dbContext.SaveChanges();
    }

    private static void RemoveDefaultUserButtonPermissions(AppDbContext dbContext, Guid userRoleId)
    {
        var removablePermissionCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CRM_HERB_BASE_ADD",
            "CRM_HERB_BASE_EDIT",
            "CRM_HERB_BASE_DELETE",
            "CRM_HERB_BASE_ASSIGN",
            "CRM_HERB_BASE_FOLLOW",
            "CRM_HERB_BASE_CONTACT_ADD",
            "CRM_HERB_BASE_CONTACT_EDIT",
            "CRM_HERB_BASE_CONTACT_PRIMARY",
            "CRM_HERB_BASE_STATUS",
            "CRM_VENDOR_ADD",
            "CRM_VENDOR_EDIT",
            "CRM_VENDOR_DELETE",
            "CRM_VENDOR_ASSIGN",
            "SYSTEM_ROLE_ADD",
            "SYSTEM_ROLE_EDIT",
            "SYSTEM_ROLE_DELETE",
            "SYSTEM_PERMISSION_ADD",
            "SYSTEM_PERMISSION_EDIT",
            "SYSTEM_PERMISSION_DELETE",
            "SYSTEM_USER_ADD",
            "SYSTEM_USER_EDIT",
            "SYSTEM_DATA_DICTIONARY_ADD",
            "SYSTEM_DATA_DICTIONARY_EDIT",
            "SYSTEM_DATA_DICTIONARY_DELETE",
            "SYSTEM_REGION_ADD",
            "SYSTEM_REGION_EDIT",
            "SYSTEM_REGION_DELETE"
        };

        var rolePermissions = (
            from rolePermission in dbContext.SystemRolePermissions
            join permission in dbContext.SystemPermissions on rolePermission.PermissionId equals permission.Id
            where rolePermission.RoleId == userRoleId && removablePermissionCodes.Contains(permission.Code)
            select rolePermission)
            .ToList();

        dbContext.SystemRolePermissions.RemoveRange(rolePermissions);
    }

    private static SystemPermission EnsurePermission(
        AppDbContext dbContext,
        List<SystemPermission> permissions,
        string name,
        string code,
        Guid? parentId)
    {
        var normalizedCode = NormalizePermissionCode(code);
        var permission = permissions.FirstOrDefault(p => NormalizePermissionCode(p.Code) == normalizedCode);

        if (permission is null)
        {
            permission = new SystemPermission(name, code, parentId);
            dbContext.SystemPermissions.Add(permission);
            permissions.Add(permission);
            return permission;
        }

        permission.Update(name, code, parentId);
        return permission;
    }

    private static void NormalizeSystemPermissions(AppDbContext dbContext)
    {
        var canonicalCodes = GetCanonicalPermissionCodes();
        var permissions = dbContext.SystemPermissions.ToList();

        foreach (var permission in permissions)
        {
            if (canonicalCodes.TryGetValue(NormalizePermissionCode(permission.Code), out var canonicalCode))
            {
                permission.Update(permission.Name, canonicalCode, permission.ParentId);
            }
        }

        dbContext.SaveChanges();
        RemoveDuplicateSystemPermissions(dbContext);
    }

    private static void RemoveDuplicateSystemPermissions(AppDbContext dbContext)
    {
        var permissions = dbContext.SystemPermissions.ToList();
        var duplicateGroups = permissions
            .GroupBy(permission => NormalizePermissionCode(permission.Code))
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Count == 0)
        {
            return;
        }

        var duplicateIdMap = new Dictionary<Guid, Guid>();

        foreach (var group in duplicateGroups)
        {
            var keeper = group
                .OrderByDescending(permission => IsCanonicalPermissionCode(permission.Code))
                .ThenBy(permission => permission.CreatedAt)
                .ThenBy(permission => permission.Id)
                .First();

            foreach (var duplicate in group.Where(permission => permission.Id != keeper.Id))
            {
                duplicateIdMap[duplicate.Id] = keeper.Id;
            }
        }

        foreach (var permission in permissions)
        {
            if (permission.ParentId.HasValue &&
                duplicateIdMap.TryGetValue(permission.ParentId.Value, out var normalizedParentId) &&
                permission.Id != normalizedParentId)
            {
                permission.Update(permission.Name, permission.Code, normalizedParentId);
            }
        }

        var rolePermissions = dbContext.SystemRolePermissions.ToList();
        foreach (var rolePermission in rolePermissions.Where(item => duplicateIdMap.ContainsKey(item.PermissionId)).ToList())
        {
            var normalizedPermissionId = duplicateIdMap[rolePermission.PermissionId];
            var exists = rolePermissions.Any(item =>
                item.RoleId == rolePermission.RoleId &&
                item.PermissionId == normalizedPermissionId);

            if (!exists)
            {
                var normalizedRolePermission = new SystemRolePermission(rolePermission.RoleId, normalizedPermissionId);
                dbContext.SystemRolePermissions.Add(normalizedRolePermission);
                rolePermissions.Add(normalizedRolePermission);
            }

            dbContext.SystemRolePermissions.Remove(rolePermission);
        }

        var duplicateIds = duplicateIdMap.Keys.ToHashSet();
        var duplicates = permissions
            .Where(permission => duplicateIds.Contains(permission.Id))
            .ToList();

        dbContext.SystemPermissions.RemoveRange(duplicates);
        dbContext.SaveChanges();
    }

    private static Dictionary<string, string> GetCanonicalPermissionCodes()
    {
        var canonicalCodes = new[]
        {
            "ROOT",
            "HOME",
            "SYSTEM",
            "SYSTEM_ROLE",
            "SYSTEM_ROLE_ADD",
            "SYSTEM_ROLE_EDIT",
            "SYSTEM_ROLE_DELETE",
            "SYSTEM_PERMISSION",
            "SYSTEM_PERMISSION_ADD",
            "SYSTEM_PERMISSION_EDIT",
            "SYSTEM_PERMISSION_DELETE",
            "SYSTEM_USER",
            "SYSTEM_USER_ADD",
            "SYSTEM_USER_EDIT",
            "SYSTEM_DATA_DICTIONARY",
            "SYSTEM_DATA_DICTIONARY_ADD",
            "SYSTEM_DATA_DICTIONARY_EDIT",
            "SYSTEM_DATA_DICTIONARY_DELETE",
            "SYSTEM_REGION",
            "SYSTEM_REGION_ADD",
            "SYSTEM_REGION_EDIT",
            "SYSTEM_REGION_DELETE",
            "SYSTEM_OPERATION_LOG",
            "CRM_HERB_BASE",
            "CRM_HERB_BASE_ADD",
            "CRM_HERB_BASE_EDIT",
            "CRM_HERB_BASE_DELETE",
            "CRM_HERB_BASE_ASSIGN",
            "CRM_HERB_BASE_FOLLOW",
            "CRM_HERB_BASE_CONTACT_ADD",
            "CRM_HERB_BASE_CONTACT_EDIT",
            "CRM_HERB_BASE_CONTACT_PRIMARY",
            "CRM_HERB_BASE_STATUS",
            "CRM_VENDOR",
            "CRM_VENDOR_ADD",
            "CRM_VENDOR_EDIT",
            "CRM_VENDOR_DELETE",
            "CRM_VENDOR_ASSIGN"
        };

        var result = canonicalCodes.ToDictionary(NormalizePermissionCode, code => code, StringComparer.Ordinal);
        var aliases = new Dictionary<string, string>
        {
            ["USERS"] = "SYSTEM_USER",
            ["USERS_ADD"] = "SYSTEM_USER_ADD",
            ["USERS_EDIT"] = "SYSTEM_USER_EDIT",
            ["ROLE"] = "SYSTEM_ROLE",
            ["ROLE_ADD"] = "SYSTEM_ROLE_ADD",
            ["ROLE_EDIT"] = "SYSTEM_ROLE_EDIT",
            ["ROLE_DELETE"] = "SYSTEM_ROLE_DELETE",
            ["PERMISSION"] = "SYSTEM_PERMISSION",
            ["PERMISSION_ADD"] = "SYSTEM_PERMISSION_ADD",
            ["PERMISSION_EDIT"] = "SYSTEM_PERMISSION_EDIT",
            ["PERMISSION_DELETE"] = "SYSTEM_PERMISSION_DELETE",
            ["DATA_DICTIONARY"] = "SYSTEM_DATA_DICTIONARY",
            ["DATA_DICTIONARY_ADD"] = "SYSTEM_DATA_DICTIONARY_ADD",
            ["DATA_DICTIONARY_EDIT"] = "SYSTEM_DATA_DICTIONARY_EDIT",
            ["DATA_DICTIONARY_DELETE"] = "SYSTEM_DATA_DICTIONARY_DELETE",
            ["REGION"] = "SYSTEM_REGION",
            ["REGION_ADD"] = "SYSTEM_REGION_ADD",
            ["REGION_EDIT"] = "SYSTEM_REGION_EDIT",
            ["REGION_DELETE"] = "SYSTEM_REGION_DELETE",
            ["OPERATION_LOG"] = "SYSTEM_OPERATION_LOG",
            ["CRM"] = "CRM_HERB_BASE",
            ["CRM_CUSTOMER"] = "CRM_HERB_BASE",
            ["CRM_CUSTOMER_ADD"] = "CRM_HERB_BASE_ADD",
            ["CRM_CUSTOMER_EDIT"] = "CRM_HERB_BASE_EDIT",
            ["CRM_CUSTOMER_DELETE"] = "CRM_HERB_BASE_DELETE",
            ["CRM_CUSTOMER_ASSIGN"] = "CRM_HERB_BASE_ASSIGN",
            ["VENDOR"] = "CRM_VENDOR",
            ["CRM_VENDOR_MANAGEMENT"] = "CRM_VENDOR"
        };

        foreach (var alias in aliases)
        {
            result[NormalizePermissionCode(alias.Key)] = alias.Value;
        }

        return result;
    }

    private static string NormalizePermissionCode(string code)
    {
        var builder = new System.Text.StringBuilder(code.Length);

        foreach (var character in code)
        {
            if (character is '_' or ':' or '-' or ' ')
            {
                continue;
            }

            builder.Append(char.ToUpperInvariant(character));
        }

        return builder.ToString();
    }

    private static bool IsCanonicalPermissionCode(string code)
    {
        return code == code.ToUpperInvariant() &&
            !code.Contains(':') &&
            !code.Contains('-') &&
            !code.Contains(' ');
    }

    private static void EnsureCrmBusinessEntityAttributesTable(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmBusinessEntityAttributes]', N'U') IS NULL
                AND OBJECT_ID(N'[BusinessEntityAttributes]', N'U') IS NOT NULL
            BEGIN
                EXEC sp_rename N'dbo.BusinessEntityAttributes', N'CrmBusinessEntityAttributes';
            END;

            IF OBJECT_ID(N'[CrmBusinessEntityAttributes]', N'U') IS NULL
            BEGIN
                CREATE TABLE [CrmBusinessEntityAttributes](
                    [Id] uniqueidentifier NOT NULL,
                    [EntityType] nvarchar(64) NOT NULL,
                    [EntityId] uniqueidentifier NOT NULL,
                    [AttributeCode] nvarchar(100) NOT NULL,
                    [AttributeValue] nvarchar(100) NOT NULL,
                    [SortOrder] int NOT NULL,
                    [Remark] nvarchar(500) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] nvarchar(max) NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    [UpdatedBy] nvarchar(max) NOT NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_CrmBusinessEntityAttributes] PRIMARY KEY ([Id])
                );
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmBusinessEntityAttributes_Entity' AND [object_id] = OBJECT_ID(N'[CrmBusinessEntityAttributes]')
            )
            BEGIN
                CREATE INDEX [IX_CrmBusinessEntityAttributes_Entity]
                ON [CrmBusinessEntityAttributes]([EntityType], [EntityId], [AttributeCode]);
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmBusinessEntityAttributes_Entity_Value' AND [object_id] = OBJECT_ID(N'[CrmBusinessEntityAttributes]')
            )
            BEGIN
                CREATE INDEX [IX_CrmBusinessEntityAttributes_Entity_Value]
                ON [CrmBusinessEntityAttributes]([EntityType], [EntityId], [AttributeCode], [AttributeValue]);
            END;

            UPDATE [CrmBusinessEntityAttributes]
            SET [EntityType] = N'CRM_HERB_BASE'
            WHERE [EntityType] = N'CRM_CUSTOMER';
            """);
    }

    private static void EnsureCrmTransferRecordsTable(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmTransferRecords]', N'U') IS NULL
            BEGIN
                CREATE TABLE [CrmTransferRecords](
                    [Id] uniqueidentifier NOT NULL,
                    [EntityType] nvarchar(64) NOT NULL,
                    [EntityId] uniqueidentifier NOT NULL,
                    [FromOwnerUserId] uniqueidentifier NULL,
                    [ToOwnerUserId] uniqueidentifier NULL,
                    [OperatorUserId] uniqueidentifier NULL,
                    [Remark] nvarchar(500) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] nvarchar(max) NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    [UpdatedBy] nvarchar(max) NOT NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_CrmTransferRecords] PRIMARY KEY ([Id])
                );
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmTransferRecords_Entity_CreatedAt'
                    AND [object_id] = OBJECT_ID(N'[CrmTransferRecords]')
            )
            BEGIN
                CREATE INDEX [IX_CrmTransferRecords_Entity_CreatedAt]
                ON [CrmTransferRecords]([EntityType], [EntityId], [CreatedAt]);
            END;

            IF OBJECT_ID(N'[CrmTransferRecords]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.CrmTransferRecords', N'FromOwnerUserName') IS NOT NULL
                    ALTER TABLE [CrmTransferRecords] DROP COLUMN [FromOwnerUserName];
                IF COL_LENGTH(N'dbo.CrmTransferRecords', N'ToOwnerUserName') IS NOT NULL
                    ALTER TABLE [CrmTransferRecords] DROP COLUMN [ToOwnerUserName];
                IF COL_LENGTH(N'dbo.CrmTransferRecords', N'OperatorUserName') IS NOT NULL
                    ALTER TABLE [CrmTransferRecords] DROP COLUMN [OperatorUserName];
            END;
            """);
    }

    private static void EnsureCrmContactsEntityColumns(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmContacts]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.CrmContacts', N'EntityType') IS NULL
                BEGIN
                    ALTER TABLE [CrmContacts] ADD [EntityType] nvarchar(64) NULL;
                END;

                IF COL_LENGTH(N'dbo.CrmContacts', N'EntityId') IS NULL
                BEGIN
                    ALTER TABLE [CrmContacts] ADD [EntityId] uniqueidentifier NULL;
                END;

                IF COL_LENGTH(N'dbo.CrmContacts', N'HerbBaseId') IS NOT NULL
                BEGIN
                    EXEC(N'
                        UPDATE [CrmContacts]
                        SET [EntityType] = N''CRM_HERB_BASE'',
                            [EntityId] = [HerbBaseId]
                        WHERE [EntityId] IS NULL;
                    ');
                END;

                DECLARE @foreignKeyName sysname;
                SELECT TOP 1 @foreignKeyName = [fk].[name]
                FROM [sys].[foreign_keys] AS [fk]
                INNER JOIN [sys].[foreign_key_columns] AS [fkc]
                    ON [fk].[object_id] = [fkc].[constraint_object_id]
                INNER JOIN [sys].[columns] AS [c]
                    ON [c].[object_id] = [fkc].[parent_object_id]
                    AND [c].[column_id] = [fkc].[parent_column_id]
                WHERE [fk].[parent_object_id] = OBJECT_ID(N'[CrmContacts]')
                    AND [c].[name] = N'HerbBaseId';

                IF @foreignKeyName IS NOT NULL
                BEGIN
                    EXEC(N'ALTER TABLE [CrmContacts] DROP CONSTRAINT [' + @foreignKeyName + N']');
                END;

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_CrmContacts_HerbBaseId'
                        AND [object_id] = OBJECT_ID(N'[CrmContacts]')
                )
                BEGIN
                    DROP INDEX [IX_CrmContacts_HerbBaseId] ON [CrmContacts];
                END;

                IF COL_LENGTH(N'dbo.CrmContacts', N'HerbBaseId') IS NOT NULL
                BEGIN
                    ALTER TABLE [CrmContacts] DROP COLUMN [HerbBaseId];
                END;

                EXEC(N'
                    UPDATE [CrmContacts]
                    SET [EntityType] = N''CRM_HERB_BASE''
                    WHERE [EntityType] = N''CRM_CUSTOMER'';
                ');

                EXEC(N'
                    UPDATE [CrmContacts]
                    SET [EntityType] = N''CRM_HERB_BASE''
                    WHERE [EntityType] IS NULL;
                ');

                EXEC(N'
                    UPDATE [CrmContacts]
                    SET [EntityId] = [Id]
                    WHERE [EntityId] IS NULL;
                ');

                EXEC(N'ALTER TABLE [CrmContacts] ALTER COLUMN [EntityType] nvarchar(64) NOT NULL;');
                EXEC(N'ALTER TABLE [CrmContacts] ALTER COLUMN [EntityId] uniqueidentifier NOT NULL;');
                ALTER TABLE [CrmContacts] ALTER COLUMN [ContactName] nvarchar(200) NOT NULL;
                ALTER TABLE [CrmContacts] ALTER COLUMN [Phone] nvarchar(100) NOT NULL;
                ALTER TABLE [CrmContacts] ALTER COLUMN [PhoneType] nvarchar(50) NOT NULL;
                ALTER TABLE [CrmContacts] ALTER COLUMN [Wechat] nvarchar(100) NOT NULL;
                ALTER TABLE [CrmContacts] ALTER COLUMN [RoleName] nvarchar(100) NOT NULL;
                ALTER TABLE [CrmContacts] ALTER COLUMN [Status] nvarchar(50) NOT NULL;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_CrmContacts_Entity'
                        AND [object_id] = OBJECT_ID(N'[CrmContacts]')
                )
                BEGIN
                    EXEC(N'CREATE INDEX [IX_CrmContacts_Entity] ON [CrmContacts]([EntityType], [EntityId]);');
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_CrmContacts_Entity_Phone'
                        AND [object_id] = OBJECT_ID(N'[CrmContacts]')
                )
                BEGIN
                    EXEC(N'CREATE INDEX [IX_CrmContacts_Entity_Phone] ON [CrmContacts]([EntityType], [EntityId], [Phone]);');
                END;
            END;
            """);
    }

    private static void EnsureCrmVendorsTable(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmVendors]', N'U') IS NULL
            BEGIN
                CREATE TABLE [CrmVendors](
                    [Id] uniqueidentifier NOT NULL,
                    [VendorName] nvarchar(200) NOT NULL,
                    [NormalizedVendorName] nvarchar(200) NOT NULL,
                    [PriorityLevel] nvarchar(20) NOT NULL,
                    [LatestPurchaseTime] datetime2 NULL,
                    [LatestPurchasePlanName] nvarchar(max) NOT NULL,
                    [Remark] nvarchar(max) NOT NULL,
                    [OwnerUserId] uniqueidentifier NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] nvarchar(max) NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    [UpdatedBy] nvarchar(max) NOT NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_CrmVendors] PRIMARY KEY ([Id])
                );
            END;

            IF OBJECT_ID(N'[CrmVendors]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmVendors', N'OwnerUserId') IS NULL
            BEGIN
                ALTER TABLE [CrmVendors] ADD [OwnerUserId] uniqueidentifier NULL;
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmVendors_NormalizedVendorName'
                    AND [object_id] = OBJECT_ID(N'[CrmVendors]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_CrmVendors_NormalizedVendorName]
                ON [CrmVendors]([NormalizedVendorName]);
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmVendors_PriorityLevel'
                    AND [object_id] = OBJECT_ID(N'[CrmVendors]')
            )
            BEGIN
                CREATE INDEX [IX_CrmVendors_PriorityLevel]
                ON [CrmVendors]([PriorityLevel]);
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmVendors_LatestPurchaseTime'
                    AND [object_id] = OBJECT_ID(N'[CrmVendors]')
            )
            BEGIN
                CREATE INDEX [IX_CrmVendors_LatestPurchaseTime]
                ON [CrmVendors]([LatestPurchaseTime]);
            END;
            """);
    }

    private static void EnsureCrmVendorPurchasePlansTable(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmVendorPurchasePlans]', N'U') IS NULL
            BEGIN
                CREATE TABLE [CrmVendorPurchasePlans](
                    [Id] uniqueidentifier NOT NULL,
                    [VendorId] uniqueidentifier NOT NULL,
                    [PurchasePlanName] nvarchar(500) NOT NULL,
                    [PurchaseTime] datetime2 NULL,
                    [Products] nvarchar(max) NOT NULL,
                    [PageUrl] nvarchar(500) NOT NULL,
                    [Remark] nvarchar(max) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] nvarchar(max) NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    [UpdatedBy] nvarchar(max) NOT NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_CrmVendorPurchasePlans] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_CrmVendorPurchasePlans_CrmVendors_VendorId]
                        FOREIGN KEY ([VendorId]) REFERENCES [CrmVendors]([Id]) ON DELETE CASCADE
                );
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmVendorPurchasePlans_VendorId'
                    AND [object_id] = OBJECT_ID(N'[CrmVendorPurchasePlans]')
            )
            BEGIN
                CREATE INDEX [IX_CrmVendorPurchasePlans_VendorId]
                ON [CrmVendorPurchasePlans]([VendorId]);
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmVendorPurchasePlans_PurchaseTime'
                    AND [object_id] = OBJECT_ID(N'[CrmVendorPurchasePlans]')
            )
            BEGIN
                CREATE INDEX [IX_CrmVendorPurchasePlans_PurchaseTime]
                ON [CrmVendorPurchasePlans]([PurchaseTime]);
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_CrmVendorPurchasePlans_PageUrl'
                    AND [object_id] = OBJECT_ID(N'[CrmVendorPurchasePlans]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_CrmVendorPurchasePlans_PageUrl]
                ON [CrmVendorPurchasePlans]([PageUrl]);
            END;
            """);
    }

    private static void EnsureCrmHerbBasesSourceIdColumn(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'SourceId') IS NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'SourceLeadId') IS NOT NULL
            BEGIN
                EXEC sp_rename N'dbo.CrmHerbBases.SourceLeadId', N'SourceId', N'COLUMN';
            END;

            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'SourceId') IS NULL
            BEGIN
                ALTER TABLE [CrmHerbBases] ADD [SourceId] bigint NULL;
            END;
            """);
    }

    private static void EnsureCrmHerbBasesBaseNameColumn(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'BaseName') IS NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'HerbBaseName') IS NOT NULL
            BEGIN
                EXEC sp_rename N'dbo.CrmHerbBases.HerbBaseName', N'BaseName', N'COLUMN';
            END;

            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'BaseName') IS NULL
            BEGIN
                ALTER TABLE [CrmHerbBases] ADD [BaseName] nvarchar(200) NOT NULL CONSTRAINT [DF_CrmHerbBases_BaseName] DEFAULT N'';
            END;
            """);
    }

    private static void EnsureCrmHerbBasesSubjectNameColumn(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'SubjectName') IS NULL
            BEGIN
                ALTER TABLE [CrmHerbBases] ADD [SubjectName] nvarchar(200) NOT NULL CONSTRAINT [DF_CrmHerbBases_SubjectName] DEFAULT N'';
            END;
            """);
    }

    private static void EnsureCrmHerbBasesScaleColumn(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CrmHerbBases', N'Scale') IS NULL
            BEGIN
                ALTER TABLE [CrmHerbBases] ADD [Scale] decimal(18,2) NULL;
            END;
            """);
    }

    private static void InitializeDataDictionaries(AppDbContext dbContext)
    {
        EnsureDefaultDataDictionaries(dbContext);
    }

    private static void EnsureDefaultDataDictionaries(AppDbContext dbContext)
    {
        var dictionaryParents = new List<DataDictionaryParentSeed>
        {
            new(
                "SYSTEM_STATUS",
                "系统状态",
                "通用系统状态",
                1,
                new[]
                {
                    new DataDictionaryItemSeed("SYSTEM_STATUS_ACTIVE", "启用", "ACTIVE", "启用状态", 1, "system_status_active"),
                    new DataDictionaryItemSeed("SYSTEM_STATUS_INACTIVE", "禁用", "INACTIVE", "禁用状态", 2, "system_status_inactive")
                },
                "system_status"),
            new(
                "ACCOUNT_STATUS",
                "账号状态",
                "通用账号状态",
                2,
                new[]
                {
                    new DataDictionaryItemSeed("ACCOUNT_STATUS_ACTIVE", "启用", "ACTIVE", "账号启用", 1, "account_status_active"),
                    new DataDictionaryItemSeed("ACCOUNT_STATUS_INACTIVE", "禁用", "INACTIVE", "账号禁用", 2, "account_status_inactive")
                },
                "account_status"),
            new(
                "CRM_HERB_BASE_GRADE",
                "CRM药材基地等级",
                "药材基地分层等级",
                101,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_HERB_BASE_GRADE_A", "A", "A", "高优先级药材基地", 1, "crm_customer_grade_a"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_GRADE_B", "B", "B", "中优先级药材基地", 2, "crm_customer_grade_b"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_GRADE_C", "C", "C", "低优先级药材基地", 3, "crm_customer_grade_c"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_GRADE_INVALID", "无效", "INVALID", "无效药材基地", 4, "crm_customer_grade_invalid")
                },
                "crm_customer_grade"),
            new(
                "CRM_HERB_BASE_STATUS",
                "CRM药材基地状态",
                "药材基地跟进状态",
                102,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_HERB_BASE_STATUS_PENDING", "待联系", "PENDING", "尚未联系", 1, "crm_customer_status_pending"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_STATUS_FOLLOWING", "跟进中", "FOLLOWING", "正在销售跟进", 2, "crm_customer_status_following"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_STATUS_INTERESTED", "有意向", "INTERESTED", "客户已表达合作意向", 3, "crm_customer_status_interested"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_STATUS_DEAL", "已成交", "DEAL", "已达成合作或成交", 4, "crm_customer_status_deal"),
                    new DataDictionaryItemSeed("CRM_HERB_BASE_STATUS_LOST", "已流失", "LOST", "药材基地已流失", 5, "crm_customer_status_lost")
                },
                "crm_customer_status"),
            new(
                "CRM_SOURCE_PLATFORM",
                "CRM来源平台",
                "药材基地线索来源平台",
                103,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_SOURCE_PLATFORM_BAIDU_MAP", "百度地图", "BAIDU_MAP", "百度地图线索", 1, "crm_source_platform_baidu_map"),
                    new DataDictionaryItemSeed("CRM_SOURCE_PLATFORM_MANUAL", "手工录入", "MANUAL", "人工新增药材基地", 2, "crm_source_platform_manual"),
                    new DataDictionaryItemSeed("CRM_SOURCE_PLATFORM_EXCEL", "Excel导入", "EXCEL", "表格导入药材基地", 3, "crm_source_platform_excel"),
                    new DataDictionaryItemSeed("CRM_SOURCE_PLATFORM_OTHER", "其他", "OTHER", "其他来源", 4, "crm_source_platform_other"),
                    new DataDictionaryItemSeed("CRM_SOURCE_PLATFORM_GOV_HERB_BASE", "政府网站", "GOV_HERB_BASE", "政府网站来源", 5, "crm_source_platform_gov_herb_base")
                },
                "crm_source_platform"),
            new(
                "CRM_MAIN_PRODUCT",
                "CRM主营品类",
                "药材基地主营药材品类",
                104,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_MAIN_PRODUCT_HUANG_QI", "黄芪", "HUANG_QI", "黄芪品类", 1, "crm_main_product_huang_qi"),
                    new DataDictionaryItemSeed("CRM_MAIN_PRODUCT_DANG_GUI", "当归", "DANG_GUI", "当归品类", 2, "crm_main_product_dang_gui"),
                    new DataDictionaryItemSeed("CRM_MAIN_PRODUCT_DANG_SHEN", "党参", "DANG_SHEN", "党参品类", 3, "crm_main_product_dang_shen"),
                    new DataDictionaryItemSeed("CRM_MAIN_PRODUCT_TIAN_MA", "天麻", "TIAN_MA", "天麻品类", 4, "crm_main_product_tian_ma"),
                    new DataDictionaryItemSeed("CRM_MAIN_PRODUCT_OTHER", "其他", "OTHER", "其他或待确认品类", 5, "crm_main_product_other")
                },
                "crm_main_product"),
            new(
                "CRM_CONTACT_PHONE_TYPE",
                "CRM联系电话类型",
                "药材基地联系人电话类型",
                104,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_CONTACT_PHONE_TYPE_MOBILE", "手机", "MOBILE", "手机号码", 1, "crm_contact_phone_type_mobile"),
                    new DataDictionaryItemSeed("CRM_CONTACT_PHONE_TYPE_LANDLINE", "座机", "LANDLINE", "固定电话", 2, "crm_contact_phone_type_landline"),
                    new DataDictionaryItemSeed("CRM_CONTACT_PHONE_TYPE_UNKNOWN", "未知", "UNKNOWN", "暂未确认号码类型", 3, "crm_contact_phone_type_unknown")
                },
                "crm_contact_phone_type"),
            new(
                "CRM_CONTACT_ROLE",
                "CRM联系人角色",
                "药材基地联系人在业务中的角色",
                105,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_CONTACT_ROLE_OWNER", "负责人", "OWNER", "药材基地负责人", 1, "crm_contact_role_owner"),
                    new DataDictionaryItemSeed("CRM_CONTACT_ROLE_PURCHASE", "采购", "PURCHASE", "采购联系人", 2, "crm_contact_role_purchase"),
                    new DataDictionaryItemSeed("CRM_CONTACT_ROLE_FINANCE", "财务", "FINANCE", "财务联系人", 3, "crm_contact_role_finance"),
                    new DataDictionaryItemSeed("CRM_CONTACT_ROLE_BASE_OWNER", "基地负责人", "BASE_OWNER", "基地负责人", 4, "crm_contact_role_base_owner"),
                    new DataDictionaryItemSeed("CRM_CONTACT_ROLE_COOPERATIVE_OWNER", "合作社负责人", "COOPERATIVE_OWNER", "合作社负责人", 5, "crm_contact_role_cooperative_owner"),
                    new DataDictionaryItemSeed("CRM_CONTACT_ROLE_OTHER", "其他", "OTHER", "其他角色", 6, "crm_contact_role_other")
                },
                "crm_contact_role"),
            new(
                "CRM_CONTACT_STATUS",
                "CRM联系人状态",
                "药材基地联系人有效性状态",
                106,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_CONTACT_STATUS_UNVERIFIED", "未验证", "UNVERIFIED", "尚未验证", 1, "crm_contact_status_unverified"),
                    new DataDictionaryItemSeed("CRM_CONTACT_STATUS_VALID", "有效", "VALID", "有效联系人", 2, "crm_contact_status_valid"),
                    new DataDictionaryItemSeed("CRM_CONTACT_STATUS_INVALID", "无效", "INVALID", "无效联系人", 3, "crm_contact_status_invalid")
                },
                "crm_contact_status"),
            new(
                "CRM_FOLLOW_TYPE",
                "CRM沟通方式",
                "销售跟进沟通方式",
                107,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_FOLLOW_TYPE_PHONE", "电话", "PHONE", "电话沟通", 1, "crm_follow_type_phone"),
                    new DataDictionaryItemSeed("CRM_FOLLOW_TYPE_WECHAT", "微信", "WECHAT", "微信沟通", 2, "crm_follow_type_wechat"),
                    new DataDictionaryItemSeed("CRM_FOLLOW_TYPE_VISIT", "拜访", "VISIT", "线下拜访", 3, "crm_follow_type_visit")
                },
                "crm_follow_type"),
            new(
                "CRM_FOLLOW_RESULT",
                "CRM沟通结果",
                "销售跟进沟通结果",
                108,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_FOLLOW_RESULT_CONNECTED", "已接通", "CONNECTED", "电话或沟通已接通", 1, "crm_follow_result_connected"),
                    new DataDictionaryItemSeed("CRM_FOLLOW_RESULT_MISSED", "未接", "MISSED", "未接听或未回复", 2, "crm_follow_result_missed"),
                    new DataDictionaryItemSeed("CRM_FOLLOW_RESULT_EMPTY_NUMBER", "空号", "EMPTY_NUMBER", "号码为空号", 3, "crm_follow_result_empty_number"),
                    new DataDictionaryItemSeed("CRM_FOLLOW_RESULT_INTERESTED", "有意向", "INTERESTED", "药材基地表达合作意向", 4, "crm_follow_result_interested"),
                    new DataDictionaryItemSeed("CRM_FOLLOW_RESULT_NOT_INTERESTED", "无意向", "NOT_INTERESTED", "药材基地暂无合作意向", 5, "crm_follow_result_not_interested")
                },
                "crm_follow_result"),
            new(
                "CRM_INTENT_LEVEL",
                "CRM意向等级",
                "药材基地沟通意向等级",
                109,
                new[]
                {
                    new DataDictionaryItemSeed("CRM_INTENT_LEVEL_A", "A", "A", "高意向", 1, "crm_intent_level_a"),
                    new DataDictionaryItemSeed("CRM_INTENT_LEVEL_B", "B", "B", "中意向", 2, "crm_intent_level_b"),
                    new DataDictionaryItemSeed("CRM_INTENT_LEVEL_C", "C", "C", "低意向", 3, "crm_intent_level_c")
                },
                "crm_intent_level")
        };

        var existingDictionaries = dbContext.SystemDataDictionaries.ToList();

        foreach (var parentSeed in dictionaryParents)
        {
            var parent = EnsureDataDictionary(
                dbContext,
                existingDictionaries,
                parentSeed.Code,
                parentSeed.Name,
                parentSeed.Code,
                parentSeed.Description,
                parentSeed.SortOrder,
                parentId: null,
                legacyCode: parentSeed.LegacyCode);

            foreach (var itemSeed in parentSeed.Items)
            {
                EnsureDataDictionary(
                    dbContext,
                    existingDictionaries,
                    itemSeed.Code,
                    itemSeed.Name,
                    itemSeed.Value,
                    itemSeed.Description,
                    itemSeed.SortOrder,
                    parent.Id,
                    itemSeed.LegacyCode);
            }
        }

        dbContext.SaveChanges();
        RemoveDuplicateDataDictionaries(dbContext);
    }

    private static SystemDataDictionary EnsureDataDictionary(
        AppDbContext dbContext,
        List<SystemDataDictionary> existingDictionaries,
        string code,
        string name,
        string value,
        string description,
        int sortOrder,
        Guid? parentId,
        string? legacyCode)
    {
        var codeKeys = new[] { code, legacyCode }
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => NormalizeDictionaryCode(item!))
            .ToHashSet(StringComparer.Ordinal);

        var dictionary = existingDictionaries.FirstOrDefault(d => codeKeys.Contains(NormalizeDictionaryCode(d.Code)));

        if (dictionary is null)
        {
            dictionary = new SystemDataDictionary(
                Guid.NewGuid(),
                code,
                name,
                value,
                description,
                sortOrder,
                true,
                parentId);

            dbContext.SystemDataDictionaries.Add(dictionary);
            existingDictionaries.Add(dictionary);

            return dictionary;
        }

        dictionary.RenameCode(code);
        dictionary.Update(name, value, description, sortOrder, true, parentId);
        return dictionary;
    }

    private static string NormalizeDictionaryCode(string code)
    {
        var builder = new System.Text.StringBuilder(code.Length);

        foreach (var character in code)
        {
            if (character is '_' or ':' or '-' or ' ')
            {
                continue;
            }

            builder.Append(char.ToUpperInvariant(character));
        }

        return builder.ToString();
    }

    private static void RemoveDuplicateDataDictionaries(AppDbContext dbContext)
    {
        var dictionaries = dbContext.SystemDataDictionaries.ToList();
        var duplicateGroups = dictionaries
            .GroupBy(item => NormalizeDictionaryCode(item.Code))
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicateGroups.Count == 0)
        {
            return;
        }

        var duplicateIdMap = new Dictionary<Guid, Guid>();

        foreach (var group in duplicateGroups)
        {
            var keeper = group
                .OrderByDescending(item => IsCanonicalDictionaryCode(item.Code))
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id)
                .First();

            foreach (var duplicate in group.Where(item => item.Id != keeper.Id))
            {
                duplicateIdMap[duplicate.Id] = keeper.Id;
            }
        }

        foreach (var dictionary in dictionaries)
        {
            if (dictionary.ParentId.HasValue &&
                duplicateIdMap.TryGetValue(dictionary.ParentId.Value, out var normalizedParentId) &&
                dictionary.Id != normalizedParentId)
            {
                dictionary.Update(
                    dictionary.Name,
                    dictionary.Value,
                    dictionary.Description,
                    dictionary.SortOrder,
                    dictionary.IsActive,
                    normalizedParentId);
            }
        }

        var duplicateIds = duplicateIdMap.Keys.ToHashSet();
        var duplicates = dictionaries
            .Where(item => duplicateIds.Contains(item.Id))
            .ToList();

        dbContext.SystemDataDictionaries.RemoveRange(duplicates);
        dbContext.SaveChanges();
    }

    private static bool IsCanonicalDictionaryCode(string code)
    {
        return code == code.ToUpperInvariant() &&
            !code.Contains(':') &&
            !code.Contains('-') &&
            !code.Contains(' ');
    }

    private sealed record DataDictionaryParentSeed(
        string Code,
        string Name,
        string Description,
        int SortOrder,
        IReadOnlyList<DataDictionaryItemSeed> Items,
        string? LegacyCode);

    private sealed record DataDictionaryItemSeed(
        string Code,
        string Name,
        string Value,
        string Description,
        int SortOrder,
        string? LegacyCode);
    private static void AddRolePermissions(
        AppDbContext dbContext,
        SystemRole role,
        IEnumerable<SystemPermission> permissions)
    {
        foreach (var permission in permissions)
        {
            dbContext.SystemRolePermissions.Add(new SystemRolePermission(role.Id, permission.Id));
        }
    }

    private static void SetParent(SystemPermission child, SystemPermission parent)
    {
        child.GetType().GetProperty("ParentId")?.SetValue(child, parent.Id);
    }

    private static void InitializeCrm(AppDbContext dbContext, List<SystemPermission> permissions)
    {
        if (dbContext.CrmHerbBases.Any())
        {
            return;
        }

        // 创建药材 CRM 测试药材基地数据
        var customers = new List<CrmHerbBase>
        {
            CrmHerbBase.Create(
                herbBaseName: "陇西黄芪种植合作社",
                grade: "A",
                score: 92,
                province: "甘肃省",
                city: "定西市",
                area: "陇西县",
                address: "甘肃省定西市陇西县首阳镇黄芪种植片区",
                lat: 35.0036m,
                lng: 104.6386m,
                sourcePlatform: "BAIDU_MAP",
                sourceId: 2001,
                ownerUserId: null,
                remark: "A类合作社，黄芪种植规模较大，需要持续跟进收购意向。"
            ),
            CrmHerbBase.Create(
                herbBaseName: "岷县当归基地",
                grade: "B",
                score: 85,
                province: "甘肃省",
                city: "定西市",
                area: "岷县",
                address: "甘肃省定西市岷县梅川镇当归种植基地",
                lat: 34.4391m,
                lng: 104.0369m,
                sourcePlatform: "BAIDU_MAP",
                sourceId: 2002,
                ownerUserId: null,
                remark: "基地电话有效，负责人上午更容易接听。"
            ),
            CrmHerbBase.Create(
                herbBaseName: "亳州药材流通商",
                grade: "B",
                score: 71,
                province: "安徽省",
                city: "亳州市",
                area: "谯城区",
                address: "安徽省亳州市谯城区药材市场周边",
                lat: 33.8446m,
                lng: 115.7793m,
                sourcePlatform: "BAIDU_MAP",
                sourceId: 2003,
                ownerUserId: null,
                remark: "流通商多品类经营，需确认黄芪和当归近期采购计划。"
            )
        };

        var subjects = customers
            .Select(customer => CrmHerbBaseSubject.Create(
                subjectName: string.Empty,
                baseName: customer.BaseName,
                subjectType: "BASE_ONLY",
                ownerUserId: customer.OwnerUserId,
                status: customer.Status,
                grade: customer.Grade,
                score: customer.Score,
                remark: customer.Remark))
            .ToList();

        dbContext.CrmHerbBaseSubjects.AddRange(subjects);
        dbContext.SaveChanges();

        for (var index = 0; index < customers.Count; index++)
        {
            customers[index].SetHerbBaseSubject(subjects[index].Id);
        }

        dbContext.CrmHerbBases.AddRange(customers);
        dbContext.SaveChanges();

        dbContext.CrmBusinessEntityAttributes.AddRange(
            new CrmBusinessEntityAttribute("CRM_HERB_BASE", customers[0].Id, "CRM_MAIN_PRODUCT", "HUANG_QI", 1),
            new CrmBusinessEntityAttribute("CRM_HERB_BASE", customers[1].Id, "CRM_MAIN_PRODUCT", "DANG_GUI", 1),
            new CrmBusinessEntityAttribute("CRM_HERB_BASE", customers[2].Id, "CRM_MAIN_PRODUCT", "OTHER", 1));
        dbContext.SaveChanges();

        var contacts = new List<CrmContact>
        {
            CrmContact.Create(
                entityType: "CRM_HERB_BASE_SUBJECT",
                entityId: subjects[0].Id,
                contactName: "王建国",
                phone: "13893210001",
                phoneType: "MOBILE",
                wechat: "wx_huangqi_wang",
                roleName: "COOPERATIVE_OWNER",
                isPrimary: true,
                remark: "主联系人，了解今年黄芪采收量。"),
            CrmContact.Create(
                entityType: "CRM_HERB_BASE_SUBJECT",
                entityId: subjects[0].Id,
                contactName: "李会计",
                phone: "13993210002",
                phoneType: "MOBILE",
                wechat: "wx_huangqi_li",
                roleName: "FINANCE",
                isPrimary: false,
                remark: "可确认结算方式。"),
            CrmContact.Create(
                entityType: "CRM_HERB_BASE_SUBJECT",
                entityId: subjects[1].Id,
                contactName: "张主任",
                phone: "13893220001",
                phoneType: "MOBILE",
                wechat: "wx_danggui_zhang",
                roleName: "BASE_OWNER",
                isPrimary: true,
                remark: "上午 9 点后方便沟通。")
        };

        dbContext.CrmContacts.AddRange(contacts);
        subjects[0].UpdatePrimaryContact(contacts[0].ContactName, contacts[0].Phone);
        subjects[1].UpdatePrimaryContact(contacts[2].ContactName, contacts[2].Phone);
        dbContext.SaveChanges();

        var nextFollowAt = DateTime.Now.Date.AddDays(2).AddHours(10);
        var followRecords = new List<CrmFollowRecord>
        {
            CrmFollowRecord.Create(
                herbBaseSubjectId: subjects[0].Id,
                herbBaseId: customers[0].Id,
                contactId: contacts[0].Id,
                followType: "PHONE",
                followResult: "CONNECTED",
                intentLevel: "A",
                content: "王建国反馈今年黄芪长势较好，预计下周能给出可供货量。",
                nextFollowAt: DateTime.Now.Date.AddDays(1).AddHours(15),
                operatorUserId: null),
            CrmFollowRecord.Create(
                herbBaseSubjectId: subjects[0].Id,
                herbBaseId: customers[0].Id,
                contactId: contacts[0].Id,
                followType: "WECHAT",
                followResult: "INTERESTED",
                intentLevel: "A",
                content: "已添加微信并发送合作资料，对方希望先确认收购价格区间。",
                nextFollowAt: nextFollowAt,
                operatorUserId: null)
        };

        dbContext.CrmFollowRecords.AddRange(followRecords);
        subjects[0].UpdateFollowSummary(DateTime.Now, "INTERESTED", nextFollowAt);
        dbContext.SaveChanges();
    }

    private static void NormalizeCrmBusinessValues(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            UPDATE [CrmHerbBases]
            SET
                [Grade] = CASE [Grade]
                    WHEN N'无效' THEN N'INVALID'
                    ELSE [Grade]
                END,
                [SourcePlatform] = CASE [SourcePlatform]
                    WHEN N'百度地图' THEN N'BAIDU_MAP'
                    WHEN N'手工录入' THEN N'MANUAL'
                    WHEN N'Excel导入' THEN N'EXCEL'
                    WHEN N'其他' THEN N'OTHER'
                    ELSE [SourcePlatform]
                END,
                [Status] = CASE [Status]
                    WHEN N'待联系' THEN N'PENDING'
                    WHEN N'跟进中' THEN N'FOLLOWING'
                    WHEN N'有意向' THEN N'INTERESTED'
                    WHEN N'已成交' THEN N'DEAL'
                    WHEN N'已流失' THEN N'LOST'
                    ELSE [Status]
                END,
                [LastFollowResult] = CASE [LastFollowResult]
                    WHEN N'已接通' THEN N'CONNECTED'
                    WHEN N'未接' THEN N'MISSED'
                    WHEN N'空号' THEN N'EMPTY_NUMBER'
                    WHEN N'有意向' THEN N'INTERESTED'
                    WHEN N'无意向' THEN N'NOT_INTERESTED'
                    ELSE [LastFollowResult]
                END;

            UPDATE [CrmContacts]
            SET
                [PhoneType] = CASE [PhoneType]
                    WHEN N'手机' THEN N'MOBILE'
                    WHEN N'座机' THEN N'LANDLINE'
                    WHEN N'未知' THEN N'UNKNOWN'
                    ELSE [PhoneType]
                END,
                [RoleName] = CASE [RoleName]
                    WHEN N'负责人' THEN N'OWNER'
                    WHEN N'采购' THEN N'PURCHASE'
                    WHEN N'财务' THEN N'FINANCE'
                    WHEN N'基地负责人' THEN N'BASE_OWNER'
                    WHEN N'合作社负责人' THEN N'COOPERATIVE_OWNER'
                    WHEN N'其他' THEN N'OTHER'
                    ELSE [RoleName]
                END,
                [Status] = CASE [Status]
                    WHEN N'未验证' THEN N'UNVERIFIED'
                    WHEN N'有效' THEN N'VALID'
                    WHEN N'无效' THEN N'INVALID'
                    ELSE [Status]
                END;

            UPDATE [CrmFollowRecords]
            SET
                [FollowType] = CASE [FollowType]
                    WHEN N'电话' THEN N'PHONE'
                    WHEN N'微信' THEN N'WECHAT'
                    WHEN N'拜访' THEN N'VISIT'
                    ELSE [FollowType]
                END,
                [FollowResult] = CASE [FollowResult]
                    WHEN N'已接通' THEN N'CONNECTED'
                    WHEN N'未接' THEN N'MISSED'
                    WHEN N'空号' THEN N'EMPTY_NUMBER'
                    WHEN N'有意向' THEN N'INTERESTED'
                    WHEN N'无意向' THEN N'NOT_INTERESTED'
                    ELSE [FollowResult]
                END;
            """);
    }

    private static void NormalizeCrmMainProducts(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[CrmHerbBases]', N'U') IS NULL
                OR COL_LENGTH(N'dbo.CrmHerbBases', N'MainProduct') IS NULL
            BEGIN
                RETURN;
            END;

            UPDATE [CrmBusinessEntityAttributes]
            SET
                [AttributeValue] = CASE
                    WHEN [AttributeValue] IN (N'黄芪', N'黃芪') THEN N'HUANG_QI'
                    WHEN [AttributeValue] IN (N'当归', N'當歸') THEN N'DANG_GUI'
                    WHEN [AttributeValue] IN (N'党参', N'黨參') THEN N'DANG_SHEN'
                    WHEN [AttributeValue] = N'天麻' THEN N'TIAN_MA'
                    WHEN [AttributeValue] IN (N'多品类', N'多品類', N'其他') THEN N'OTHER'
                    ELSE [AttributeValue]
                END,
                [UpdatedAt] = SYSUTCDATETIME(),
                [UpdatedBy] = N'System'
            WHERE [EntityType] = N'CRM_HERB_BASE'
                AND [AttributeCode] = N'CRM_MAIN_PRODUCT'
                AND [IsDeleted] = 0
                AND [AttributeValue] IN (
                    N'黄芪',
                    N'黃芪',
                    N'当归',
                    N'當歸',
                    N'党参',
                    N'黨參',
                    N'天麻',
                    N'多品类',
                    N'多品類',
                    N'其他'
                );

            WITH [DuplicatedAttributes] AS (
                SELECT
                    [Id],
                    ROW_NUMBER() OVER (
                        PARTITION BY [EntityType], [EntityId], [AttributeCode], [AttributeValue]
                        ORDER BY [SortOrder], [CreatedAt], [Id]
                    ) AS [RowNumber]
                FROM [CrmBusinessEntityAttributes]
                WHERE [EntityType] = N'CRM_HERB_BASE'
                    AND [AttributeCode] = N'CRM_MAIN_PRODUCT'
                    AND [IsDeleted] = 0
            )
            UPDATE [Attribute]
            SET
                [IsDeleted] = 1,
                [UpdatedAt] = SYSUTCDATETIME(),
                [UpdatedBy] = N'System'
            FROM [CrmBusinessEntityAttributes] AS [Attribute]
            INNER JOIN [DuplicatedAttributes] AS [Duplicated]
                ON [Duplicated].[Id] = [Attribute].[Id]
            WHERE [Duplicated].[RowNumber] > 1;

            ALTER TABLE [CrmHerbBases] DROP COLUMN [MainProduct];
            """);
    }

    private static void EnsureDefaultCrmOwner(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            DECLARE @AdminUserId uniqueidentifier;

            SELECT TOP 1 @AdminUserId = [Id]
            FROM [SystemUsers]
            WHERE [Username] = N'admin' AND [IsDeleted] = 0
            ORDER BY [CreatedAt];

            IF @AdminUserId IS NOT NULL
            BEGIN
                UPDATE [CrmHerbBases]
                SET
                    [OwnerUserId] = @AdminUserId,
                    [UpdatedAt] = SYSUTCDATETIME(),
                    [UpdatedBy] = N'System'
                WHERE [IsDeleted] = 0 AND [OwnerUserId] IS NULL;
            END;
            """);
    }
}





