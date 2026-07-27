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
        if (dbContext.SystemPermissions.Any())
        {
            return dbContext.SystemPermissions.ToList();
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
        var operationLog = new SystemPermission("操作日志", "operationLog");

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
        SetParent(operationLog, system);

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
            operationLog,
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

    private static void InitializeCrm(AppDbContext dbContext, List<SystemPermission> permissions)
    {
        if (dbContext.CrmCustomers.Any())
        {
            return;
        }

        // 创建药材 CRM 测试客户数据
        var customers = new List<CrmCustomer>
        {
            CrmCustomer.Create(
                customerName: "陇西黄芪种植合作社",
                customerType: "合作社",
                mainProduct: "黄芪",
                grade: "A",
                score: 92,
                province: "甘肃省",
                city: "定西市",
                area: "陇西县",
                address: "甘肃省定西市陇西县首阳镇黄芪种植片区",
                lat: 35.0036m,
                lng: 104.6386m,
                sourcePlatform: "百度地图",
                sourceLeadId: 2001,
                ownerUserId: null,
                remark: "A类合作社，黄芪种植规模较大，需要持续跟进收购意向。",
                parentCustomerId: null
            ),
            CrmCustomer.Create(
                customerName: "岷县当归基地",
                customerType: "基地",
                mainProduct: "当归",
                grade: "B",
                score: 85,
                province: "甘肃省",
                city: "定西市",
                area: "岷县",
                address: "甘肃省定西市岷县梅川镇当归种植基地",
                lat: 34.4391m,
                lng: 104.0369m,
                sourcePlatform: "百度地图",
                sourceLeadId: 2002,
                ownerUserId: null,
                remark: "基地电话有效，负责人上午更容易接听。",
                parentCustomerId: null
            ),
            CrmCustomer.Create(
                customerName: "亳州药材流通商",
                customerType: "流通商",
                mainProduct: "多品类",
                grade: "B",
                score: 71,
                province: "安徽省",
                city: "亳州市",
                area: "谯城区",
                address: "安徽省亳州市谯城区药材市场周边",
                lat: 33.8446m,
                lng: 115.7793m,
                sourcePlatform: "百度地图",
                sourceLeadId: 2003,
                ownerUserId: null,
                remark: "流通商多品类经营，需确认黄芪和当归近期采购计划。",
                parentCustomerId: null
            )
        };

        dbContext.CrmCustomers.AddRange(customers);
        dbContext.SaveChanges();

        var contacts = new List<CrmContact>
        {
            CrmContact.Create(
                customerId: customers[0].Id,
                contactName: "王建国",
                phone: "13893210001",
                phoneType: "手机",
                wechat: "wx_huangqi_wang",
                roleName: "合作社负责人",
                isPrimary: true,
                remark: "主联系人，了解今年黄芪采收量。"),
            CrmContact.Create(
                customerId: customers[0].Id,
                contactName: "李会计",
                phone: "13993210002",
                phoneType: "手机",
                wechat: "wx_huangqi_li",
                roleName: "财务",
                isPrimary: false,
                remark: "可确认结算方式。"),
            CrmContact.Create(
                customerId: customers[1].Id,
                contactName: "张主任",
                phone: "13893220001",
                phoneType: "手机",
                wechat: "wx_danggui_zhang",
                roleName: "基地负责人",
                isPrimary: true,
                remark: "上午 9 点后方便沟通。")
        };

        dbContext.CrmContacts.AddRange(contacts);
        customers[0].UpdatePrimaryContact(contacts[0].ContactName, contacts[0].Phone);
        customers[1].UpdatePrimaryContact(contacts[2].ContactName, contacts[2].Phone);
        dbContext.SaveChanges();

        var nextFollowAt = DateTime.Now.Date.AddDays(2).AddHours(10);
        var followRecords = new List<CrmFollowRecord>
        {
            CrmFollowRecord.Create(
                customerId: customers[0].Id,
                contactId: contacts[0].Id,
                followType: "电话",
                followResult: "已接通",
                intentLevel: "A",
                content: "王建国反馈今年黄芪长势较好，预计下周能给出可供货量。",
                nextFollowAt: DateTime.Now.Date.AddDays(1).AddHours(15),
                operatorUserId: null),
            CrmFollowRecord.Create(
                customerId: customers[0].Id,
                contactId: contacts[0].Id,
                followType: "微信",
                followResult: "有意向",
                intentLevel: "A",
                content: "已添加微信并发送合作资料，对方希望先确认收购价格区间。",
                nextFollowAt: nextFollowAt,
                operatorUserId: null)
        };

        dbContext.CrmFollowRecords.AddRange(followRecords);
        customers[0].UpdateFollowSummary(DateTime.Now, "有意向", nextFollowAt);
        dbContext.SaveChanges();
    }
}
