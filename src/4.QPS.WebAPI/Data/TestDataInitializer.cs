using QPS.Domain.Entities;
using QPS.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        var merchant = InitializeMerchant(dbContext);

        if (merchant == null)
            return;

        var shops = InitializeShops(dbContext, merchant);
        var roles = InitializeRoles(dbContext, merchant);
        InitializeUsers(dbContext, merchant, roles);
        InitializePermissions(dbContext, merchant);
        InitializeRolePermissions(dbContext, merchant, roles);
        var tags = InitializeTags(dbContext, merchant);
        var rooms = InitializeRooms(dbContext, shops, merchant);
        InitializeRoomImages(dbContext, rooms, merchant);
        var plans = InitializePlans(dbContext, merchant);
        InitializeRoomTags(dbContext, rooms, tags, merchant);
        InitializeRoomPlans(dbContext, rooms, plans, merchant);
        InitializeCoupons(dbContext, merchant);
        var customers = InitializeCustomers(dbContext, merchant);
        InitializeOrders(dbContext, shops, rooms, customers, merchant);
        InitializeCustomerCoupons(dbContext, customers, merchant);
        InitializeReviews(dbContext, merchant);
    }

    private static Merchant InitializeMerchant(AppDbContext dbContext)
    {
        // 使用 IgnoreQueryFilters() 绕过全局查询过滤器
        var existingMerchant = dbContext.Merchants.IgnoreQueryFilters().FirstOrDefault();
        if (existingMerchant != null)
        {
            return existingMerchant;
        }

        var merchant = Merchant.Create("测试商户", "13800138000", DateTime.Now.AddYears(1));
        dbContext.Merchants.Add(merchant);
        dbContext.SaveChanges();
        return merchant;
    }

    private static List<Shop> InitializeShops(AppDbContext dbContext, Merchant merchant)
    {
        // 使用 IgnoreQueryFilters() 绕过全局查询过滤器
        var existingShops = dbContext.Shops.IgnoreQueryFilters().Where(s => s.MerchantId == merchant.Id).ToList();
        if (existingShops.Any())
            return existingShops;

        var shops = new List<Shop>
        {
            Shop.Create("旗舰店", "北京市朝阳区xxx路1号", "010-12345678", new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 30),
            Shop.Create("分店A", "北京市海淀区xxx路2号", "010-23456789", new TimeSpan(10, 0, 0), new TimeSpan(21, 0, 0), 20),
            Shop.Create("分店B", "北京市西城区xxx路3号", "010-34567890", new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0), 45),
            Shop.Create("分店C", "北京市东城区xxx路4号", "010-45678901", new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 35),
            Shop.Create("分店D", "北京市丰台区xxx路5号", "010-56789012", new TimeSpan(8, 30, 0), new TimeSpan(21, 30, 0), 25),
            Shop.Create("分店E", "北京市石景山区xxx路6号", "010-67890123", new TimeSpan(10, 0, 0), new TimeSpan(20, 0, 0), 40)
        };

        foreach (var shop in shops)
        {
            shop.GetType().GetProperty("MerchantId")?.SetValue(shop, merchant.Id);
        }

        dbContext.Shops.AddRange(shops);
        dbContext.SaveChanges();
        return shops;
    }

    private static List<SystemRole> InitializeRoles(AppDbContext dbContext, Merchant merchant)
    {
        var existingRoles = dbContext.SystemRoles.IgnoreQueryFilters().Where(r => r.MerchantId == merchant.Id).ToList();
        if (existingRoles.Any())
            return existingRoles;

        var roles = new List<SystemRole>
        {
            new SystemRole("管理员", "admin"),
            new SystemRole("店长", "shop_manager"),
            new SystemRole("收银员", "cashier")
        };

        foreach (var role in roles)
        {
            role.GetType().GetProperty("MerchantId")?.SetValue(role, merchant.Id);
        }

        dbContext.SystemRoles.AddRange(roles);
        dbContext.SaveChanges();
        return roles;
    }

    private static void InitializeUsers(AppDbContext dbContext, Merchant merchant, List<SystemRole> roles)
    {
        var existingUsers = dbContext.SystemUsers.IgnoreQueryFilters().Where(u => u.MerchantId == merchant.Id).ToList();
        if (existingUsers.Any())
            return;

        var adminRole = roles.First(r => r.Code == "admin");
        var managerRole = roles.First(r => r.Code == "shop_manager");

        var users = new List<SystemUser>
        {
            SystemUser.Create("admin", "123456", "系统管理员"),
            SystemUser.Create("manager", "123456", "张店长"),
            SystemUser.Create("cashier1", "123456", "李收银员")
        };

        foreach (var user in users)
        {
            user.GetType().GetProperty("MerchantId")?.SetValue(user, merchant.Id);
        }

        dbContext.SystemUsers.AddRange(users);
        dbContext.SaveChanges();

        var userRoles = new List<SystemUserRole>
        {
            new SystemUserRole(users[0].Id, adminRole.Id),
            new SystemUserRole(users[1].Id, managerRole.Id),
            new SystemUserRole(users[2].Id, roles.First(r => r.Code == "cashier").Id)
        };

        dbContext.SystemUserRoles.AddRange(userRoles);
        dbContext.SaveChanges();
    }

    private static List<Tag> InitializeTags(AppDbContext dbContext, Merchant merchant)
    {
        var existingTags = dbContext.Tags.IgnoreQueryFilters().Where(t => t.MerchantId == merchant.Id).ToList();
        if (existingTags.Any())
            return existingTags;

        var tags = new List<Tag>
        {
            new Tag("VIP", "等级"),
            new Tag("特惠", "促销"),
            new Tag("热门", "推荐"),
            new Tag("安静", "环境"),
            new Tag("宽敞", "环境"),
            new Tag("温馨", "环境"),
            new Tag("情侣", "主题"),
            new Tag("商务", "主题"),
            new Tag("电竞", "主题"),
            new Tag("影音", "设施")
        };

        foreach (var tag in tags)
        {
            tag.GetType().GetProperty("MerchantId")?.SetValue(tag, merchant.Id);
        }

        dbContext.Tags.AddRange(tags);
        dbContext.SaveChanges();
        return tags;
    }

    private static List<Room> InitializeRooms(AppDbContext dbContext, List<Shop> shops, Merchant merchant)
    {
        var existingRooms = dbContext.Rooms.IgnoreQueryFilters().Where(r => r.MerchantId == merchant.Id).ToList();
        if (existingRooms.Any())
            return existingRooms;

        var rooms = new List<Room>();
        var random = new Random();
        var statuses = new[] { "Idle", "Occupied", "Cleaning", "Fault" };
        var roomCount = 30; // 增加到30个房间

        for (int i = 0; i < roomCount; i++)
        {
            var shop = shops[i % shops.Count];
            var floor = (char)('A' + (i / 5));
            var roomNumber = $"{floor}{(i % 5 + 1).ToString("00")}";
            var price = 68.00m + random.Next(0, 8) * 15;
            var isEnabled = random.Next(0, 10) != 0; // 90%启用

            var room = Room.Create(shop.Id, roomNumber, price, isEnabled);

            // 设置不同状态，覆盖所有状态
            var statusIndex = i % statuses.Length;
            room.GetType().GetProperty("Status")?.SetValue(room, Enum.Parse(typeof(Domain.Entities.RoomStatus), statuses[statusIndex]));

            room.GetType().GetProperty("MerchantId")?.SetValue(room, merchant.Id);
            rooms.Add(room);
        }

        dbContext.Rooms.AddRange(rooms);
        dbContext.SaveChanges();
        return rooms;
    }

    private static void InitializeRoomImages(AppDbContext dbContext, List<Room> rooms, Merchant merchant)
    {
        if (dbContext.RoomImages.Any())
            return;

        var images = new List<RoomImage>();

        foreach (var room in rooms.Take(5))
        {
            var image1 = new RoomImage(room.Id, $"https://example.com/images/room/{room.Id}-1.jpg", true, 1);
            image1.GetType().GetProperty("MerchantId")?.SetValue(image1, merchant.Id);

            var image2 = new RoomImage(room.Id, $"https://example.com/images/room/{room.Id}-2.jpg", false, 2);
            image2.GetType().GetProperty("MerchantId")?.SetValue(image2, merchant.Id);

            var image3 = new RoomImage(room.Id, $"https://example.com/images/room/{room.Id}-3.jpg", false, 3);
            image3.GetType().GetProperty("MerchantId")?.SetValue(image3, merchant.Id);

            images.Add(image1);
            images.Add(image2);
            images.Add(image3);
        }

        dbContext.RoomImages.AddRange(images);
        dbContext.SaveChanges();
    }

    private static List<Plan> InitializePlans(AppDbContext dbContext, Merchant merchant)
    {
        var existingPlans = dbContext.Plans.IgnoreQueryFilters().Where(p => p.MerchantId == merchant.Id).ToList();
        if (existingPlans.Any())
            return existingPlans;

        var plans = new List<Plan>
        {
            new Plan("体验套餐", "30分钟体验", 38.00m, 30),
            new Plan("标准套餐", "1小时标准服务", 68.00m, 60),
            new Plan("豪华套餐", "2小时豪华服务", 128.00m, 120),
            new Plan("通宵套餐", "8小时通宵服务", 298.00m, 480)
        };

        foreach (var plan in plans)
        {
            plan.GetType().GetProperty("MerchantId")?.SetValue(plan, merchant.Id);
        }

        dbContext.Plans.AddRange(plans);
        dbContext.SaveChanges();
        return plans;
    }

    private static void InitializeCoupons(AppDbContext dbContext, Merchant merchant)
    {
        var existingCoupons = dbContext.Coupons.IgnoreQueryFilters().Where(c => c.MerchantId == merchant.Id).ToList();
        if (existingCoupons.Any())
            return;

        var coupons = new List<Coupon>
        {
            new Coupon("新人优惠券", "fixed", 10.00m, 50.00m, DateTime.Now.AddMonths(3)),
            new Coupon("满减优惠", "fixed", 20.00m, 100.00m, DateTime.Now.AddMonths(2)),
            new Coupon("会员折扣", "percent", 10.00m, 0, DateTime.Now.AddMonths(1))
        };

        foreach (var coupon in coupons)
        {
            coupon.GetType().GetProperty("MerchantId")?.SetValue(coupon, merchant.Id);
        }

        dbContext.Coupons.AddRange(coupons);
        dbContext.SaveChanges();
    }

    private static List<Customer> InitializeCustomers(AppDbContext dbContext, Merchant merchant)
    {
        if (dbContext.Customers.Any())
            return dbContext.Customers.ToList();

        var customers = new List<Customer>
        {
            new Customer("openid_001", "13900139001", "张三", "https://example.com/avatar/1.jpg"),
            new Customer("openid_002", "13900139002", "李四", "https://example.com/avatar/2.jpg"),
            new Customer("openid_003", "13900139003", "王五", "https://example.com/avatar/3.jpg"),
            new Customer("openid_004", "13900139004", "赵六", "https://example.com/avatar/4.jpg"),
            new Customer("openid_005", "13900139005", "钱七", "https://example.com/avatar/5.jpg")
        };

        foreach (var customer in customers)
        {
            customer.GetType().GetProperty("MerchantId")?.SetValue(customer, merchant.Id);
        }

        dbContext.Customers.AddRange(customers);
        dbContext.SaveChanges();
        return customers;
    }

    private static void InitializeOrders(AppDbContext dbContext, List<Shop> shops, List<Room> rooms, List<Customer> customers, Merchant merchant)
    {
        if (dbContext.Orders.Any())
            return;

        var orders = new List<Order>();
        var random = new Random();

        for (int i = 0; i < 500; i++) // 增加订单数量到500
        {
            var room = rooms[random.Next(rooms.Count)];
            var shop = shops.First(s => s.Id == room.ShopId);
            var customer = customers[random.Next(customers.Count)];

            var order = Order.Create(shop.Id, room.Id, customer.Id);
            order.GetType().GetProperty("MerchantId")?.SetValue(order, merchant.Id);

            orders.Add(order);
        }

        dbContext.Orders.AddRange(orders);
        dbContext.SaveChanges();

        // 完成80%的订单
        var completedOrders = orders.Take((int)(orders.Count * 0.8)).ToList();
        foreach (var order in completedOrders)
        {
            var amount = 68.00m + random.Next(0, 10) * 15;
            order.Start();
            order.Complete(amount, amount * 0.1m, amount * 0.9m, "wechat");
        }

        dbContext.SaveChanges();

        foreach (var order in orders)
        {
            var item1 = new OrderItem(order.Id, "基础服务", order.OriginAmount * 0.8m, 1);
            item1.GetType().GetProperty("MerchantId")?.SetValue(item1, merchant.Id);

            var item2 = new OrderItem(order.Id, "附加服务", order.OriginAmount * 0.2m, 1);
            item2.GetType().GetProperty("MerchantId")?.SetValue(item2, merchant.Id);

            dbContext.OrderItems.Add(item1);
            dbContext.OrderItems.Add(item2);
        }

        dbContext.SaveChanges();

        // 在最后一个 SaveChanges 之后设置订单时间，时间跨度3个月
        var today = DateTime.UtcNow.Date;
        var threeMonthsAgo = today.AddMonths(-3);
        foreach (var order in orders)
        {
            // 设置创建时间在3个月内随机分布
            var daysBetween = (today - threeMonthsAgo).Days;
            var randomDays = random.Next(daysBetween + 1);
            var createdAt = threeMonthsAgo.AddDays(randomDays);
            createdAt = createdAt.AddHours(random.Next(24)).AddMinutes(random.Next(60));
            dbContext.Entry(order).Property("CreatedAt").CurrentValue = createdAt;

            if (order.Status == OrderStatus.Completed)
            {
                // 设置支付时间在创建时间之后，最多7天内
                var paidDaysAfter = random.Next(0, 8);
                var paidAt = createdAt.AddDays(paidDaysAfter);
                if (paidAt > DateTime.UtcNow)
                    paidAt = DateTime.UtcNow;
                dbContext.Entry(order).Property("PaidAt").CurrentValue = paidAt;
            }
        }

        dbContext.SaveChanges();
    }

    private static void InitializeRoomTags(AppDbContext dbContext, List<Room> rooms, List<Tag> tags, Merchant merchant)
    {
        if (dbContext.RoomTags.Any())
            return;

        var roomTags = new List<RoomTag>();
        var random = new Random();

        foreach (var room in rooms)
        {
            var tagCount = random.Next(1, 4);
            var selectedTags = tags.OrderBy(t => Guid.NewGuid()).Take(tagCount).ToList();

            foreach (var tag in selectedTags)
            {
                var roomTag = new RoomTag(room.Id, tag.Id);
                roomTag.GetType().GetProperty("MerchantId")?.SetValue(roomTag, merchant.Id);
                roomTags.Add(roomTag);
            }
        }

        dbContext.RoomTags.AddRange(roomTags);
        dbContext.SaveChanges();
    }

    private static void InitializeRoomPlans(AppDbContext dbContext, List<Room> rooms, List<Plan> plans, Merchant merchant)
    {
        if (dbContext.RoomPlans.Any())
            return;

        var roomPlans = new List<RoomPlan>();
        var random = new Random();

        foreach (var room in rooms)
        {
            var planCount = random.Next(1, 3);
            var selectedPlans = plans.OrderBy(p => Guid.NewGuid()).Take(planCount).ToList();

            foreach (var plan in selectedPlans)
            {
                var roomPlan = new RoomPlan(room.Id, plan.Id);
                roomPlan.GetType().GetProperty("MerchantId")?.SetValue(roomPlan, merchant.Id);
                roomPlans.Add(roomPlan);
            }
        }

        dbContext.RoomPlans.AddRange(roomPlans);
        dbContext.SaveChanges();
    }

    private static void InitializeCustomerCoupons(AppDbContext dbContext, List<Customer> customers, Merchant merchant)
    {
        if (dbContext.CustomerCoupons.Any())
            return;

        var coupons = dbContext.Coupons.IgnoreQueryFilters().Where(c => c.MerchantId == merchant.Id).ToList();
        if (!coupons.Any())
            return;

        var random = new Random();
        var customerCoupons = new List<CustomerCoupon>();

        foreach (var customer in customers)
        {
            // 每个客户随机分配1-3张优惠券
            var couponCount = random.Next(1, 4);
            var selectedCoupons = coupons.OrderBy(c => Guid.NewGuid()).Take(couponCount).ToList();

            foreach (var coupon in selectedCoupons)
            {
                // 随机设置状态：未使用、已使用、已过期
                var statuses = new[] { "unused", "used", "expired" };
                var status = statuses[random.Next(statuses.Length)];

                var customerCoupon = new CustomerCoupon(coupon.Id, customer.Id, status);
                customerCoupon.GetType().GetProperty("MerchantId")?.SetValue(customerCoupon, merchant.Id);
                customerCoupons.Add(customerCoupon);
            }
        }

        dbContext.CustomerCoupons.AddRange(customerCoupons);
        dbContext.SaveChanges();
    }

    private static void InitializeReviews(AppDbContext dbContext, Merchant merchant)
    {
        if (dbContext.Reviews.Any())
            return;

        // 获取已完成的订单（用于关联评价）
        var completedOrders = dbContext.Orders
            .IgnoreQueryFilters()
            .Where(o => o.MerchantId == merchant.Id && o.Status == OrderStatus.Completed)
            .ToList();

        if (!completedOrders.Any())
            return;

        var random = new Random();
        var reviews = new List<Review>();
        var comments = new[]
        {
            "非常满意，环境很好！",
            "服务态度很棒，下次还来。",
            "房间很干净，设施齐全。",
            "性价比很高，推荐！",
            "体验不错，值得再来。",
            "房间有点小，但整体不错。",
            "服务很周到，赞！",
            "环境优雅，适合约会。",
            "价格合理，服务到位。",
            "整体体验很好，会推荐给朋友。"
        };

        // 为一部分已完成的订单创建评价
        var ordersForReview = completedOrders.OrderBy(o => Guid.NewGuid()).Take(30).ToList();

        foreach (var order in ordersForReview)
        {
            var score = random.Next(1, 6); // 1-5分
            var content = comments[random.Next(comments.Length)];

            var review = new Review(order.Id, order.RoomId, order.CustomerId.Value, score, content);
            review.GetType().GetProperty("MerchantId")?.SetValue(review, merchant.Id);
            reviews.Add(review);

            // 更新房间评分
            var room = dbContext.Rooms.IgnoreQueryFilters().FirstOrDefault(r => r.Id == order.RoomId);
            if (room != null)
            {
                room.AddRating((decimal)score);
            }
        }

        dbContext.Reviews.AddRange(reviews);
        dbContext.SaveChanges();
    }

    private static void InitializePermissions(AppDbContext dbContext, Merchant merchant)
    {
        var existingPermissions = dbContext.SystemPermissions.IgnoreQueryFilters().Where(p => p.MerchantId == merchant.Id).ToList();
        if (existingPermissions.Any())
            return;

        // 1. 先创建父级分组权限
        var parentPermissions = new List<SystemPermission>
        {
            new SystemPermission("order", "订单管理"),
            new SystemPermission("room", "房间管理"),
            new SystemPermission("coupon", "优惠券管理"),
            new SystemPermission("user", "用户管理"),
            new SystemPermission("role", "角色管理"),
            new SystemPermission("shop", "店铺管理"),
            new SystemPermission("settings", "系统设置"),
            new SystemPermission("permission", "权限管理"),
        };

        foreach (var p in parentPermissions)
        {
            p.GetType().GetProperty("MerchantId")?.SetValue(p, merchant.Id);
        }

        dbContext.SystemPermissions.AddRange(parentPermissions);
        dbContext.SaveChanges();

        // 2. 查询获取父级 ID 映射
        var savedPermissions = dbContext.SystemPermissions.IgnoreQueryFilters()
            .Where(p => p.MerchantId == merchant.Id)
            .ToDictionary(p => p.PermissionCode);

        // 3. 创建子权限（功能点）
        var childPermissions = new List<SystemPermission>
        {
            // 订单管理
            new SystemPermission("order:view", "订单查看", savedPermissions["order"].Id),
            new SystemPermission("order:create", "创建订单", savedPermissions["order"].Id),
            new SystemPermission("order:edit", "编辑订单", savedPermissions["order"].Id),
            new SystemPermission("order:delete", "删除订单", savedPermissions["order"].Id),
            new SystemPermission("order:settle", "结算订单", savedPermissions["order"].Id),

            // 房间管理
            new SystemPermission("room:view", "房间查看", savedPermissions["room"].Id),
            new SystemPermission("room:create", "创建房间", savedPermissions["room"].Id),
            new SystemPermission("room:edit", "编辑房间", savedPermissions["room"].Id),
            new SystemPermission("room:delete", "删除房间", savedPermissions["room"].Id),

            // 优惠券管理
            new SystemPermission("coupon:view", "优惠券查看", savedPermissions["coupon"].Id),
            new SystemPermission("coupon:create", "创建优惠券", savedPermissions["coupon"].Id),
            new SystemPermission("coupon:edit", "编辑优惠券", savedPermissions["coupon"].Id),
            new SystemPermission("coupon:delete", "删除优惠券", savedPermissions["coupon"].Id),

            // 用户管理
            new SystemPermission("user:view", "用户查看", savedPermissions["user"].Id),
            new SystemPermission("user:create", "创建用户", savedPermissions["user"].Id),
            new SystemPermission("user:edit", "编辑用户", savedPermissions["user"].Id),

            // 角色管理
            new SystemPermission("role:view", "角色查看", savedPermissions["role"].Id),
            new SystemPermission("role:create", "创建角色", savedPermissions["role"].Id),
            new SystemPermission("role:edit", "编辑角色", savedPermissions["role"].Id),
            new SystemPermission("role:delete", "删除角色", savedPermissions["role"].Id),

            // 店铺管理
            new SystemPermission("shop:view", "店铺查看", savedPermissions["shop"].Id),
            new SystemPermission("shop:create", "创建店铺", savedPermissions["shop"].Id),
            new SystemPermission("shop:edit", "编辑店铺", savedPermissions["shop"].Id),
            new SystemPermission("shop:delete", "删除店铺", savedPermissions["shop"].Id),

            // 系统设置
            new SystemPermission("settings:view", "设置查看", savedPermissions["settings"].Id),
            new SystemPermission("settings:edit", "修改设置", savedPermissions["settings"].Id),

            // 权限管理
            new SystemPermission("permission:view", "权限查看", savedPermissions["permission"].Id),
            new SystemPermission("permission:create", "创建权限", savedPermissions["permission"].Id),
            new SystemPermission("permission:edit", "编辑权限", savedPermissions["permission"].Id),
            new SystemPermission("permission:delete", "删除权限", savedPermissions["permission"].Id),
        };

        foreach (var p in childPermissions)
        {
            p.GetType().GetProperty("MerchantId")?.SetValue(p, merchant.Id);
        }

        dbContext.SystemPermissions.AddRange(childPermissions);
        dbContext.SaveChanges();
    }

    private static void InitializeRolePermissions(AppDbContext dbContext, Merchant merchant, List<SystemRole> roles)
    {
        var permissions = dbContext.SystemPermissions.IgnoreQueryFilters().Where(p => p.MerchantId == merchant.Id).ToList();
        var existingRolePermissions = dbContext.SystemRolePermissions.IgnoreQueryFilters().Where(rp => rp.MerchantId == merchant.Id).ToList();
        if (existingRolePermissions.Any())
            return;

        var adminRole = roles.First(r => r.Code == "admin");
        var managerRole = roles.First(r => r.Code == "shop_manager");
        var cashierRole = roles.First(r => r.Code == "cashier");

        var rolePermissions = new List<SystemRolePermission>();

        // 管理员：所有权限
        foreach (var permission in permissions)
        {
            rolePermissions.Add(new SystemRolePermission(adminRole.Id, permission.Id));
        }

        // 店长：订单、房间、优惠券、店铺相关权限
        var managerPermissionCodes = new[] { "order:", "room:", "coupon:", "shop:" };
        var managerPermissions = permissions.Where(p =>
            managerPermissionCodes.Any(prefix => p.PermissionCode.StartsWith(prefix))).ToList();
        foreach (var permission in managerPermissions)
        {
            rolePermissions.Add(new SystemRolePermission(managerRole.Id, permission.Id));
        }

        // 收银员：订单相关权限
        var cashierPermissions = permissions.Where(p =>
            p.PermissionCode.StartsWith("order:")).ToList();
        foreach (var permission in cashierPermissions)
        {
            rolePermissions.Add(new SystemRolePermission(cashierRole.Id, permission.Id));
        }

        foreach (var rp in rolePermissions)
        {
            rp.GetType().GetProperty("MerchantId")?.SetValue(rp, merchant.Id);
        }

        dbContext.SystemRolePermissions.AddRange(rolePermissions);
        dbContext.SaveChanges();
    }
}
