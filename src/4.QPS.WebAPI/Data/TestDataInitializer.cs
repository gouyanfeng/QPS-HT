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

    private static void InitializePermissions(AppDbContext dbContext, List<SystemRole> roles)
    {
        if (dbContext.SystemPermissions.Any())
        {
            return;
        }

        var root = new SystemPermission("权限管理", "root");
        var home = new SystemPermission("首页", "home");
        var system = new SystemPermission("系统设置", "system");
        var users = new SystemPermission("用户管理", "users");
        var usersAdd = new SystemPermission("新增", "users:add");
        var usersEdit = new SystemPermission("编辑", "users:edit");
        var role = new SystemPermission("角色设置", "role");
        var roleAdd = new SystemPermission("新增", "role:add");
        var roleEdit = new SystemPermission("编辑", "role:edit");
        var roleDelete = new SystemPermission("删除", "role:delete");
        var permission = new SystemPermission("权限设置", "permission");
        var permissionAdd = new SystemPermission("新增", "permission:add");
        var permissionEdit = new SystemPermission("编辑", "permission:edit");
        var permissionDelete = new SystemPermission("删除", "permission:delete");
        var dataDictionary = new SystemPermission("数据字典", "dataDictionary");
        var dataDictionaryAdd = new SystemPermission("新增", "dataDictionary:add");
        var dataDictionaryEdit = new SystemPermission("编辑", "dataDictionary:edit");
        var dataDictionaryDelete = new SystemPermission("删除", "dataDictionary:delete");

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
            root, home, system,
            users, usersAdd, usersEdit,
            role, roleAdd, roleEdit, roleDelete,
            permission, permissionAdd, permissionEdit, permissionDelete,
            dataDictionary, dataDictionaryAdd, dataDictionaryEdit, dataDictionaryDelete
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
            new(Guid.NewGuid(), "system_status", "系统状态", "active", "通用系统状态", 1, true),
            new(Guid.NewGuid(), "account_status", "账户状态", "active", "通用账户状态", 2, true)
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
