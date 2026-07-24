using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities.System;
using QPS.Infrastructure.Database;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        var roles = InitializeRoles(dbContext);
        InitializeUsers(dbContext, roles);
        InitializePermissions(dbContext, roles);
        InitializeDataDictionaries(dbContext);
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
            new("Administrator", "admin"),
            new("User", "user")
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
            SystemUser.Create("admin", "123456", "Administrator", adminRole.Id),
            SystemUser.Create("user", "123456", "User", userRole.Id)
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

    private static void InitializePermissions(AppDbContext dbContext, List<SystemRole> roles)
    {
        if (dbContext.SystemPermissions.Any())
        {
            return;
        }

        var root = new SystemPermission("Permission Management", "root");
        var home = new SystemPermission("Home", "home");
        var system = new SystemPermission("System", "system");
        var users = new SystemPermission("Users", "users");
        var usersAdd = new SystemPermission("Add", "users:add");
        var usersEdit = new SystemPermission("Edit", "users:edit");
        var role = new SystemPermission("Roles", "role");
        var roleAdd = new SystemPermission("Add", "role:add");
        var roleEdit = new SystemPermission("Edit", "role:edit");
        var roleDelete = new SystemPermission("Delete", "role:delete");
        var permission = new SystemPermission("Permissions", "permission");
        var permissionAdd = new SystemPermission("Add", "permission:add");
        var permissionEdit = new SystemPermission("Edit", "permission:edit");
        var permissionDelete = new SystemPermission("Delete", "permission:delete");
        var dataDictionary = new SystemPermission("Data Dictionary", "dataDictionary");
        var dataDictionaryAdd = new SystemPermission("Add", "dataDictionary:add");
        var dataDictionaryEdit = new SystemPermission("Edit", "dataDictionary:edit");
        var dataDictionaryDelete = new SystemPermission("Delete", "dataDictionary:delete");

        SetParent(home, root);
        SetParent(system, root);
        SetParent(users, system);
        SetParent(usersAdd, users);
        SetParent(usersEdit, users);
        SetParent(role, system);
        SetParent(roleAdd, role);
        SetParent(roleEdit, role);
        SetParent(roleDelete, role);
        SetParent(permission, system);
        SetParent(permissionAdd, permission);
        SetParent(permissionEdit, permission);
        SetParent(permissionDelete, permission);
        SetParent(dataDictionary, system);
        SetParent(dataDictionaryAdd, dataDictionary);
        SetParent(dataDictionaryEdit, dataDictionary);
        SetParent(dataDictionaryDelete, dataDictionary);

        var permissions = new List<SystemPermission>
        {
            root,
            home,
            system,
            users,
            usersAdd,
            usersEdit,
            role,
            roleAdd,
            roleEdit,
            roleDelete,
            permission,
            permissionAdd,
            permissionEdit,
            permissionDelete,
            dataDictionary,
            dataDictionaryAdd,
            dataDictionaryEdit,
            dataDictionaryDelete
        };

        dbContext.SystemPermissions.AddRange(permissions);
        dbContext.SaveChanges();

        var adminRole = roles.First(r => r.Code == "admin");
        var userRole = roles.First(r => r.Code == "user");

        AddRolePermissions(dbContext, adminRole, permissions.Where(p => p.Code != "root"));
        AddRolePermissions(dbContext, userRole, permissions.Where(p => p.Code == "home"));

        dbContext.SaveChanges();
    }

    private static void InitializeDataDictionaries(AppDbContext dbContext)
    {
        if (dbContext.SystemDataDictionaries.Any())
        {
            return;
        }

        var dictionaries = new List<SystemDataDictionary>
        {
            new(Guid.NewGuid(), "system_status", "System Status", "active", "Generic system status", 1, true),
            new(Guid.NewGuid(), "account_status", "Account Status", "active", "Generic account status", 2, true)
        };

        dbContext.SystemDataDictionaries.AddRange(dictionaries);
        dbContext.SaveChanges();
    }

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
}
