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
        InitializeDataDictionaries(dbContext);
        InitializeCrm(dbContext, permissions);
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

    private static List<SystemPermission> InitializePermissions(AppDbContext dbContext, List<SystemRole> roles)
    {
        if (dbContext.SystemPermissions.Any())
        {
            return dbContext.SystemPermissions.ToList();
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

        // CRM 权限
        var crm = new SystemPermission("CRM", "crm");
        var crmCustomer = new SystemPermission("Customers", "crm:customer");
        var crmCustomerAdd = new SystemPermission("Add", "crm:customer:add");
        var crmCustomerEdit = new SystemPermission("Edit", "crm:customer:edit");
        var crmCustomerDelete = new SystemPermission("Delete", "crm:customer:delete");

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

        // CRM 权限层级
        SetParent(crm, root);
        SetParent(crmCustomer, crm);
        SetParent(crmCustomerAdd, crmCustomer);
        SetParent(crmCustomerEdit, crmCustomer);
        SetParent(crmCustomerDelete, crmCustomer);

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
            dataDictionaryDelete,
            crm,
            crmCustomer,
            crmCustomerAdd,
            crmCustomerEdit,
            crmCustomerDelete
        };

        dbContext.SystemPermissions.AddRange(permissions);
        dbContext.SaveChanges();

        var adminRole = roles.First(r => r.Code == "admin");
        var userRole = roles.First(r => r.Code == "user");

        AddRolePermissions(dbContext, adminRole, permissions.Where(p => p.Code != "root"));
        AddRolePermissions(dbContext, userRole, permissions.Where(p => p.Code == "home"));

        dbContext.SaveChanges();

        return permissions;
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

    private static void InitializeCrm(AppDbContext dbContext, List<SystemPermission> permissions)
    {
        if (dbContext.CrmCustomers.Any())
        {
            return;
        }

        // 创建测试客户数据
        var customers = new List<CrmCustomer>
        {
            CrmCustomer.Create(
                customerName: "北京科技有限公司",
                customerType: "企业客户",
                mainProduct: "软件开发",
                grade: "A",
                score: 1000,
                province: "北京市",
                city: "北京市",
                area: "朝阳区",
                address: "北京市朝阳区科技园区A座",
                lat: 39.9042m,
                lng: 116.4074m,
                sourcePlatform: "官网",
                sourceLeadId: 1001,
                ownerUserId: null,
                remark: "重要客户，需要重点跟进",
                parentCustomerId: null
            ),
            CrmCustomer.Create(
                customerName: "上海贸易集团",
                customerType: "企业客户",
                mainProduct: "进出口贸易",
                grade: "A",
                score: 850,
                province: "上海市",
                city: "上海市",
                area: "浦东新区",
                address: "上海市浦东新区陆家嘴金融中心",
                lat: 31.2304m,
                lng: 121.4737m,
                sourcePlatform: "展会",
                sourceLeadId: 1002,
                ownerUserId: null,
                remark: "长期合作伙伴",
                parentCustomerId: null
            ),
            CrmCustomer.Create(
                customerName: "广州制造有限公司",
                customerType: "企业客户",
                mainProduct: "机械设备",
                grade: "B",
                score: 600,
                province: "广东省",
                city: "广州市",
                area: "天河区",
                address: "广州市天河区工业园区",
                lat: 23.1291m,
                lng: 113.2644m,
                sourcePlatform: "电话营销",
                sourceLeadId: 1003,
                ownerUserId: null,
                remark: "潜力客户",
                parentCustomerId: null
            ),
            CrmCustomer.Create(
                customerName: "深圳市创新科技",
                customerType: "企业客户",
                mainProduct: "电子产品",
                grade: "B",
                score: 550,
                province: "广东省",
                city: "深圳市",
                area: "南山区",
                address: "深圳市南山区科技园",
                lat: 22.5431m,
                lng: 114.0579m,
                sourcePlatform: "线上广告",
                sourceLeadId: 1004,
                ownerUserId: null,
                remark: "新兴科技公司",
                parentCustomerId: null
            ),
            CrmCustomer.Create(
                customerName: "李小明",
                customerType: "个人客户",
                mainProduct: "个人服务",
                grade: "C",
                score: 200,
                province: "浙江省",
                city: "杭州市",
                area: "西湖区",
                address: "杭州市西湖区文三路",
                lat: 30.2741m,
                lng: 120.1552m,
                sourcePlatform: "社交媒体",
                sourceLeadId: 1005,
                ownerUserId: null,
                remark: "个人用户",
                parentCustomerId: null
            )
        };

        dbContext.CrmCustomers.AddRange(customers);
        dbContext.SaveChanges();
    }
}