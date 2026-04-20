using QPS.Domain.Aggregates.MerchantAggregate;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.RoomAggregate;
using QPS.Domain.Aggregates.UserAggregate;
using QPS.Domain.Aggregates.PricingAggregate;
using QPS.Domain.Aggregates.OperationAggregate;
using QPS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        // 检查是否已有数据
        if (dbContext.Merchants.Any())
            return;

        // 1. 创建测试商户
        var merchants = new List<Merchant>
        {
            new Merchant("测试商户1", "13800138001", DateTime.UtcNow.AddYears(1)),
            new Merchant("测试商户2", "13800138002", DateTime.UtcNow.AddYears(1)),
            new Merchant("测试商户3", "13800138003", DateTime.UtcNow.AddYears(1))
        };
        dbContext.Merchants.AddRange(merchants);
        dbContext.SaveChanges();

        // 2. 为每个商户创建1个门店
        var shops = new List<Shop>();
        foreach (var merchant in merchants)
        {
            var shop = new Shop(
                merchant.Id,
                $"{merchant.Name}门店",
                $"测试地址{merchants.IndexOf(merchant) + 1}",
                new TimeSpan(9, 0, 0),
                new TimeSpan(22, 0, 0),
                30
            );
            shops.Add(shop);
        }
        dbContext.Shops.AddRange(shops);
        dbContext.SaveChanges();

        // 3. 创建权限数据
        var permissions = new List<Permission>
        {
            new Permission("查看商户", "merchant:view"),
            new Permission("管理商户", "merchant:manage"),
            new Permission("查看门店", "shop:view"),
            new Permission("管理门店", "shop:manage"),
            new Permission("查看房间", "room:view"),
            new Permission("管理房间", "room:manage"),
            new Permission("查看订单", "order:view"),
            new Permission("管理订单", "order:manage"),
            new Permission("查看用户", "user:view"),
            new Permission("管理用户", "user:manage")
        };
        dbContext.Permissions.AddRange(permissions);
        dbContext.SaveChanges();

        // 4. 为每个商户创建角色
        var roles = new List<Role>();
        foreach (var merchant in merchants)
        {
            roles.Add(new Role(merchant.Id, "管理员", "admin"));
            roles.Add(new Role(merchant.Id, "操作员", "operator"));
        }
        dbContext.Roles.AddRange(roles);
        dbContext.SaveChanges();

        // 5. 创建角色权限映射
        var rolePermissions = new List<RolePermission>();
        foreach (var role in roles)
        {
            if (role.Code == "admin")
            {
                // 管理员拥有所有权限
                foreach (var permission in permissions)
                {
                    rolePermissions.Add(new RolePermission(role.Id, permission.Code));
                }
            }
            else if (role.Code == "operator")
            {
                // 操作员拥有部分权限
                var operatorPermissions = permissions.Where(p => 
                    p.Code.EndsWith(":view") || 
                    p.Code == "order:manage" || 
                    p.Code == "room:manage"
                ).ToList();
                foreach (var permission in operatorPermissions)
                {
                    rolePermissions.Add(new RolePermission(role.Id, permission.Code));
                }
            }
        }
        dbContext.RolePermissions.AddRange(rolePermissions);
        dbContext.SaveChanges();

        // 6. 为每个商户创建用户
        var users = new List<User>();
        foreach (var merchant in merchants)
        {
            // 为每个商户创建一个管理员用户
            users.Add(new User(merchant.Id, $"admin{merchants.IndexOf(merchant) + 1}", "Password123!", $"管理员{merchants.IndexOf(merchant) + 1}"));
            // 为每个商户创建一个操作员用户
            users.Add(new User(merchant.Id, $"operator{merchants.IndexOf(merchant) + 1}", "Password123!", $"操作员{merchants.IndexOf(merchant) + 1}"));
        }
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        // 7. 创建用户角色映射
        var userRoles = new List<UserRole>();
        foreach (var merchant in merchants)
        {
            var merchantUsers = users.Where(u => u.MerchantId == merchant.Id).ToList();
            var merchantRoles = roles.Where(r => r.MerchantId == merchant.Id).ToList();
            
            // 第一个用户为管理员
            userRoles.Add(new UserRole(merchantUsers[0].Id, merchantRoles.First(r => r.Code == "admin").Id));
            // 第二个用户为操作员
            userRoles.Add(new UserRole(merchantUsers[1].Id, merchantRoles.First(r => r.Code == "operator").Id));
        }
        dbContext.UserRoles.AddRange(userRoles);
        dbContext.SaveChanges();

        // 8. 为每个门店创建房间
        var rooms = new List<Room>();
        foreach (var shop in shops)
        {
            for (int i = 1; i <= 5; i++)
            {
                var room = new Room(
                    shop.MerchantId,
                    shop.Id,
                    $"房间{i}",
                    $"DEVICE{shop.Id.ToString().Substring(0, 8)}{i}",
                    $"room/{shop.MerchantId}/{shop.Id}/room{i}",100
                );
                rooms.Add(room);
            }
        }
        dbContext.Rooms.AddRange(rooms);
        dbContext.SaveChanges();

        // 9. 为每个商户创建标签
        var tags = new List<Tag>();
        foreach (var merchant in merchants)
        {
            tags.Add(new Tag(merchant.Id, "VIP", "等级"));
            tags.Add(new Tag(merchant.Id, "普通", "等级"));
            tags.Add(new Tag(merchant.Id, "无烟", "环境"));
            tags.Add(new Tag(merchant.Id, "有烟", "环境"));
            tags.Add(new Tag(merchant.Id, "靠窗", "位置"));
        }
        dbContext.Tags.AddRange(tags);
        dbContext.SaveChanges();

        // 10. 为房间创建标签映射
        var roomTagMappings = new List<RoomTagMapping>();
        var random = new Random();
        foreach (var room in rooms)
        {
            var merchantTags = tags.Where(t => t.MerchantId == room.MerchantId).ToList();
            // 为每个房间随机分配2-3个标签
            var tagCount = random.Next(2, 4);
            var selectedTags = merchantTags.OrderBy(t => random.Next()).Take(tagCount).ToList();
            foreach (var tag in selectedTags)
            {
                roomTagMappings.Add(new RoomTagMapping(room.Id, tag.Id));
            }
        }
        dbContext.RoomTagMappings.AddRange(roomTagMappings);
        dbContext.SaveChanges();

        // 11. 为每个房间创建图片
        var roomImages = new List<RoomImage>();
        foreach (var room in rooms)
        {
            for (int i = 1; i <= 3; i++)
            {
                var roomImage = new RoomImage(room.Id, $"room_{room.Id}_image_{i}.jpg", true, i);
                roomImages.Add(roomImage);
            }
        }
        dbContext.RoomImages.AddRange(roomImages);
        dbContext.SaveChanges();

        // 12. 为每个商户创建套餐
        var plans = new List<Plan>();
        foreach (var merchant in merchants)
        {
            plans.Add(new Plan(merchant.Id, "小时套餐", "1小时使用时间", 30, 60));
            plans.Add(new Plan(merchant.Id, "半天套餐", "4小时使用时间", 100, 240));
            plans.Add(new Plan(merchant.Id, "全天套餐", "8小时使用时间", 180, 480));
            plans.Add(new Plan(merchant.Id, "深夜套餐", "2小时使用时间（22:00-08:00）", 40, 120));
        }
        dbContext.Plans.AddRange(plans);
        dbContext.SaveChanges();

        // 14. 为每个商户创建优惠券
        var coupons = new List<Coupon>();
        foreach (var merchant in merchants)
        {

            coupons.Add(new Coupon(merchant.Id, "满100减20", "满100元可用，立减20元", 20, 100, DateTime.UtcNow.AddMonths(1)));
            coupons.Add(new Coupon(merchant.Id, "满200减50", "满200元可用，立减50元", 50, 200, DateTime.UtcNow.AddMonths(1)));
            coupons.Add(new Coupon(merchant.Id, "无门槛券", "无最低消费限制，立减10元", 10, 0, DateTime.UtcNow.AddMonths(1)));
        }
        dbContext.Coupons.AddRange(coupons);
        dbContext.SaveChanges();

        // 15. 创建客户
        var customers = new List<Customer>();
        for (int i = 1; i <= 10; i++)
        {
            
            customers.Add(new Customer($"客户{i}", $"138001380{10 + i}", DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"), $"客户{i}"));
        }
        dbContext.Customers.AddRange(customers);
        dbContext.SaveChanges();

        // 16. 为客户分配优惠券
        var customerCoupons = new List<CustomerCoupon>();
        foreach (var customer in customers)
        {
            var merchantCoupons = coupons.Where(c => c.MerchantId == merchants[0].Id).ToList();
            foreach (var coupon in merchantCoupons)
            {
                customerCoupons.Add(new CustomerCoupon(customer.Id, coupon.Id, "false"));
            }
        }
        dbContext.CustomerCoupons.AddRange(customerCoupons);
        dbContext.SaveChanges();

        // 17. 创建折扣
        var discounts = new List<Discount>();
        foreach (var merchant in merchants)
        {
            discounts.Add(new Discount(merchant.Id, "周末特惠", 20, DateTime.UtcNow.AddMonths(1),DateTime.UtcNow.AddMonths(1)));
            discounts.Add(new Discount(merchant.Id, "节假日特惠", 30, DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(1))); 
            discounts.Add(new Discount(merchant.Id, "会员特惠", 10, DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(1)));
        }
        dbContext.Discounts.AddRange(discounts);
        dbContext.SaveChanges();

        // 18. 为每个商户创建订单
        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();
        int orderCounter = 1;
        
        foreach (var shop in shops)
        {
            var shopRooms = rooms.Where(r => r.ShopId == shop.Id).ToList();
            for (int i = 1; i <= 50; i++)
            {
                var room = shopRooms[random.Next(shopRooms.Count)];
                var customer = customers[random.Next(customers.Count)];
                
                var order = new Order(
                    $"ORD{DateTime.Now:yyyyMMddHHmmssfff}{orderCounter++}",
                    shop.MerchantId,
                    shop.Id,
                    room.Id,
                    customer.Id
                );
                order.Start();
                order.Complete(100, 20, 80, "CASH");
                orders.Add(order);
                
                // 创建订单项
                orderItems.Add(new OrderItem(order.Id, "房费", 100, 1));
                
          
          
            }
        }
        dbContext.Orders.AddRange(orders);
        dbContext.OrderItems.AddRange(orderItems);
        dbContext.SaveChanges();

        // 19. 创建设备操作日志
        var deviceOperationLogs = new List<DeviceOperationLog>();
        foreach (var room in rooms)
        {
            for (int i = 1; i <= 5; i++)
            {
                var log =   new DeviceOperationLog(
                    room.Id,
                    "POWER_ON",
                    "系统自动开机"
                   
                );
                deviceOperationLogs.Add(log);
                
                log = new DeviceOperationLog(
                    room.Id,
                    "POWER_OFF",
                    "系统自动关机"
                );
                deviceOperationLogs.Add(log);
            }
        }
        dbContext.DeviceOperationLogs.AddRange(deviceOperationLogs);
        dbContext.SaveChanges();
    }
}