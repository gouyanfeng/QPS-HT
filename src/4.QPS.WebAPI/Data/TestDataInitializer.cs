using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;
using QPS.Domain.Entities.System;
using QPS.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        var merchant = InitializeMerchant(dbContext);

        if (merchant == null)
            return;

        var shops = InitializeShops(dbContext);
        var roles = InitializeRoles(dbContext);
        InitializeUsers(dbContext, roles);
        InitializePermissions(dbContext, roles);
        var tags = InitializeTags(dbContext);
        var rooms = InitializeRooms(dbContext, shops);
        InitializeRoomImages(dbContext, rooms);
        var plans = InitializePlans(dbContext);
        InitializeRoomTags(dbContext, rooms, tags);
        InitializeRoomPlans(dbContext, rooms, plans);
        InitializeCoupons(dbContext);
        var customers = InitializeCustomers(dbContext);
        InitializeOrders(dbContext, shops, rooms, customers);
        InitializeCustomerCoupons(dbContext, customers);
        InitializeReviews(dbContext);
        InitializeDataDictionaries(dbContext);
    }

    private static Merchant? InitializeMerchant(AppDbContext dbContext)
    {
        var existingMerchant = dbContext.Merchants.FirstOrDefault();
        if (existingMerchant != null)
        {
            return existingMerchant;
        }

        var merchant = Merchant.Create("测试商户", "13800138000", DateTime.Now.AddYears(1));
        dbContext.Merchants.Add(merchant);
        dbContext.SaveChanges();
        return merchant;
    }

    private static List<Shop> InitializeShops(AppDbContext dbContext)
    {
        if (dbContext.Shops.Any())
            return dbContext.Shops.ToList();

        var shops = new List<Shop>
        {
            Shop.Create("旗舰店", "北京市朝阳区xxx路1号", "010-12345678", new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 30),
            Shop.Create("分店A", "北京市海淀区xxx路2号", "010-23456789", new TimeSpan(10, 0, 0), new TimeSpan(21, 0, 0), 20),
            Shop.Create("分店B", "北京市西城区xxx路3号", "010-34567890", new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0), 45),
            Shop.Create("分店C", "北京市东城区xxx路4号", "010-45678901", new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 35),
            Shop.Create("分店D", "北京市丰台区xxx路5号", "010-56789012", new TimeSpan(8, 30, 0), new TimeSpan(21, 30, 0), 25),
            Shop.Create("分店E", "北京市石景山区xxx路6号", "010-67890123", new TimeSpan(10, 0, 0), new TimeSpan(20, 0, 0), 40)
        };

        dbContext.Shops.AddRange(shops);
        dbContext.SaveChanges();
        return shops;
    }

    private static List<SystemRole> InitializeRoles(AppDbContext dbContext)
    {
        if (dbContext.SystemRoles.Any())
            return dbContext.SystemRoles.ToList();

        var roles = new List<SystemRole>
        {
            new SystemRole("管理员", "admin"),
            new SystemRole("商户", "merchant"),
            new SystemRole("用户", "user")
        };

        dbContext.SystemRoles.AddRange(roles);
        dbContext.SaveChanges();
        return roles;
    }

    private static void InitializeUsers(AppDbContext dbContext, List<SystemRole> roles)
    {
        if (dbContext.SystemUsers.Any())
            return;

        var adminRole = roles.First(r => r.Code == "admin");
        var merchantRole = roles.First(r => r.Code == "merchant");
        var userRole = roles.First(r => r.Code == "user");

        var users = new List<SystemUser>
        {
            SystemUser.Create("admin", "123456", "系统管理员", adminRole.Id),
            SystemUser.Create("merchant", "123456", "张商户", merchantRole.Id),
            SystemUser.Create("user", "123456", "李用户", userRole.Id)
        };

        dbContext.SystemUsers.AddRange(users);
        dbContext.SaveChanges();

        var userRoles = new List<SystemUserRole>
        {
            new SystemUserRole(users[0].Id, adminRole.Id),
            new SystemUserRole(users[1].Id, merchantRole.Id),
            new SystemUserRole(users[2].Id, userRole.Id)
        };

        dbContext.SystemUserRoles.AddRange(userRoles);
        dbContext.SaveChanges();
    }

    private static List<Tag> InitializeTags(AppDbContext dbContext)
    {
        if (dbContext.Tags.Any())
            return dbContext.Tags.ToList();

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

        dbContext.Tags.AddRange(tags);
        dbContext.SaveChanges();
        return tags;
    }

    private static List<Room> InitializeRooms(AppDbContext dbContext, List<Shop> shops)
    {
        if (dbContext.Rooms.Any())
            return dbContext.Rooms.ToList();

        var rooms = new List<Room>();
        var random = new Random();
        var statuses = new[] { "Idle", "Occupied", "Cleaning", "Fault" };
        var roomCount = 30;

        for (int i = 0; i < roomCount; i++)
        {
            var shop = shops[i % shops.Count];
            var floor = (char)('A' + (i / 5));
            var roomNumber = $"{floor}{(i % 5 + 1).ToString("00")}";
            var price = 68.00m + random.Next(0, 8) * 15;
            var isEnabled = random.Next(0, 10) != 0;

            var room = Room.Create(shop.Id, roomNumber, price, isEnabled);

            var statusIndex = i % statuses.Length;
            room.GetType().GetProperty("Status")?.SetValue(room, Enum.Parse(typeof(RoomStatus), statuses[statusIndex]));

            rooms.Add(room);
        }

        dbContext.Rooms.AddRange(rooms);
        dbContext.SaveChanges();
        return rooms;
    }

    private static void InitializeRoomImages(AppDbContext dbContext, List<Room> rooms)
    {
        if (dbContext.RoomImages.Any())
            return;

        var images = new List<RoomImage>();

        foreach (var room in rooms.Take(5))
        {
            var image1 = new RoomImage(room.Id, $"https://example.com/images/room/{room.Id}-1.jpg", true, 1);
            var image2 = new RoomImage(room.Id, $"https://example.com/images/room/{room.Id}-2.jpg", false, 2);
            var image3 = new RoomImage(room.Id, $"https://example.com/images/room/{room.Id}-3.jpg", false, 3);

            images.Add(image1);
            images.Add(image2);
            images.Add(image3);
        }

        dbContext.RoomImages.AddRange(images);
        dbContext.SaveChanges();
    }

    private static List<Plan> InitializePlans(AppDbContext dbContext)
    {
        if (dbContext.Plans.Any())
            return dbContext.Plans.ToList();

        var plans = new List<Plan>
        {
            new Plan("体验套餐", "30分钟体验", 38.00m, 30),
            new Plan("标准套餐", "1小时标准服务", 68.00m, 60),
            new Plan("豪华套餐", "2小时豪华服务", 128.00m, 120),
            new Plan("通宵套餐", "8小时通宵服务", 298.00m, 480)
        };

        dbContext.Plans.AddRange(plans);
        dbContext.SaveChanges();
        return plans;
    }

    private static void InitializeCoupons(AppDbContext dbContext)
    {
        if (dbContext.Coupons.Any())
            return;

        var coupons = new List<Coupon>
        {
            new Coupon("新人优惠券", "fixed", 10.00m, 50.00m, DateTime.Now.AddMonths(3)),
            new Coupon("满减优惠", "fixed", 20.00m, 100.00m, DateTime.Now.AddMonths(2)),
            new Coupon("会员折扣", "percent", 10.00m, 0, DateTime.Now.AddMonths(1))
        };

        dbContext.Coupons.AddRange(coupons);
        dbContext.SaveChanges();
    }

    private static List<Customer> InitializeCustomers(AppDbContext dbContext)
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

        dbContext.Customers.AddRange(customers);
        dbContext.SaveChanges();
        return customers;
    }

    private static void InitializeOrders(AppDbContext dbContext, List<Shop> shops, List<Room> rooms, List<Customer> customers)
    {
        if (dbContext.Orders.Any())
            return;

        var orders = new List<Order>();
        var random = new Random();

        for (int i = 0; i < 500; i++)
        {
            var room = rooms[random.Next(rooms.Count)];
            var shop = shops.First(s => s.Id == room.ShopId);
            var customer = customers[random.Next(customers.Count)];

            var order = Order.Create(shop.Id, room.Id, customer.Id);
            orders.Add(order);
        }

        dbContext.Orders.AddRange(orders);
        dbContext.SaveChanges();

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
            var item2 = new OrderItem(order.Id, "附加服务", order.OriginAmount * 0.2m, 1);
            dbContext.OrderItems.Add(item1);
            dbContext.OrderItems.Add(item2);
        }

        dbContext.SaveChanges();

        var today = DateTime.UtcNow.Date;
        var threeMonthsAgo = today.AddMonths(-3);
        foreach (var order in orders)
        {
            var daysBetween = (today - threeMonthsAgo).Days;
            var randomDays = random.Next(daysBetween + 1);
            var createdAt = threeMonthsAgo.AddDays(randomDays);
            createdAt = createdAt.AddHours(random.Next(24)).AddMinutes(random.Next(60));
            dbContext.Entry(order).Property("CreatedAt").CurrentValue = createdAt;

            if (order.Status == OrderStatus.Completed)
            {
                var paidDaysAfter = random.Next(0, 8);
                var paidAt = createdAt.AddDays(paidDaysAfter);
                if (paidAt > DateTime.UtcNow)
                    paidAt = DateTime.UtcNow;
                dbContext.Entry(order).Property("PaidAt").CurrentValue = paidAt;
            }
        }

        dbContext.SaveChanges();
    }

    private static void InitializeRoomTags(AppDbContext dbContext, List<Room> rooms, List<Tag> tags)
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
                roomTags.Add(roomTag);
            }
        }

        dbContext.RoomTags.AddRange(roomTags);
        dbContext.SaveChanges();
    }

    private static void InitializeRoomPlans(AppDbContext dbContext, List<Room> rooms, List<Plan> plans)
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
                roomPlans.Add(roomPlan);
            }
        }

        dbContext.RoomPlans.AddRange(roomPlans);
        dbContext.SaveChanges();
    }

    private static void InitializeCustomerCoupons(AppDbContext dbContext, List<Customer> customers)
    {
        if (dbContext.CustomerCoupons.Any())
            return;

        var coupons = dbContext.Coupons.ToList();
        if (!coupons.Any())
            return;

        var random = new Random();
        var customerCoupons = new List<CustomerCoupon>();

        foreach (var customer in customers)
        {
            var couponCount = random.Next(1, 4);
            var selectedCoupons = coupons.OrderBy(c => Guid.NewGuid()).Take(couponCount).ToList();

            foreach (var coupon in selectedCoupons)
            {
                var statuses = new[] { "unused", "used", "expired" };
                var status = statuses[random.Next(statuses.Length)];

                var customerCoupon = new CustomerCoupon(coupon.Id, customer.Id, status);
                customerCoupons.Add(customerCoupon);
            }
        }

        dbContext.CustomerCoupons.AddRange(customerCoupons);
        dbContext.SaveChanges();
    }

    private static void InitializePermissions(AppDbContext dbContext, List<SystemRole> roles)
    {
        if (dbContext.SystemPermissions.Any())
            return;

        var permList = new List<SystemPermission>
        {
            new SystemPermission("权限管理", "root"),
            new SystemPermission("首页", "home"),
            new SystemPermission("商户管理", "merchants"),
            new SystemPermission("新增", "merchants:add"),
            new SystemPermission("编辑", "merchants:edit"),
            new SystemPermission("门店管理", "shops"),
            new SystemPermission("新增", "shops:add"),
            new SystemPermission("编辑", "shops:edit"),
            new SystemPermission("删除", "shops:delete"),
            new SystemPermission("房间管理", "rooms"),
            new SystemPermission("新增", "rooms:add"),
            new SystemPermission("编辑", "rooms:edit"),
            new SystemPermission("优惠券管理", "coupons"),
            new SystemPermission("新增", "coupons:add"),
            new SystemPermission("编辑", "coupons:edit"),
            new SystemPermission("删除", "coupons:delete"),
            new SystemPermission("套餐管理", "plans"),
            new SystemPermission("新增", "plans:add"),
            new SystemPermission("编辑", "plans:edit"),
            new SystemPermission("删除", "plans:delete"),
            new SystemPermission("订单管理", "orders"),
            new SystemPermission("编辑", "orders:edit"),
            new SystemPermission("查看", "orders:view"),
            new SystemPermission("标签管理", "tags"),
            new SystemPermission("新增", "tags:add"),
            new SystemPermission("编辑", "tags:edit"),
            new SystemPermission("删除", "tags:delete"),
            new SystemPermission("用户管理", "users"),
            new SystemPermission("新增", "users:add"),
            new SystemPermission("编辑", "users:edit"),
            new SystemPermission("系统设置", "system"),
            new SystemPermission("角色设置", "role"),
            new SystemPermission("新增", "role:add"),
            new SystemPermission("编辑", "role:edit"),
            new SystemPermission("删除", "role:delete"),
            new SystemPermission("权限设置", "permission"),
            new SystemPermission("新增", "permission:add"),
            new SystemPermission("编辑", "permission:edit"),
            new SystemPermission("删除", "permission:delete"),
            new SystemPermission("数据字典", "dataDictionary"),
            new SystemPermission("新增", "dataDictionary:add"),
            new SystemPermission("编辑", "dataDictionary:edit"),
            new SystemPermission("删除", "dataDictionary:delete"),
        };

        dbContext.SystemPermissions.AddRange(permList);
        dbContext.SaveChanges();

        var permDict = dbContext.SystemPermissions
            .Where(p => !p.IsDeleted)
            .ToDictionary(p => p.Code);

        SetParent(permDict, "home", "root");
        SetParent(permDict, "merchants", "root");
        SetParent(permDict, "shops", "root");
        SetParent(permDict, "rooms", "root");
        SetParent(permDict, "coupons", "root");
        SetParent(permDict, "plans", "root");
        SetParent(permDict, "orders", "root");
        SetParent(permDict, "tags", "root");
        SetParent(permDict, "users", "system");
        SetParent(permDict, "system", "root");

        SetParent(permDict, "merchants:add", "merchants");
        SetParent(permDict, "merchants:edit", "merchants");
        SetParent(permDict, "shops:add", "shops");
        SetParent(permDict, "shops:edit", "shops");
        SetParent(permDict, "shops:delete", "shops");
        SetParent(permDict, "rooms:add", "rooms");
        SetParent(permDict, "rooms:edit", "rooms");
        SetParent(permDict, "coupons:add", "coupons");
        SetParent(permDict, "coupons:edit", "coupons");
        SetParent(permDict, "coupons:delete", "coupons");
        SetParent(permDict, "plans:add", "plans");
        SetParent(permDict, "plans:edit", "plans");
        SetParent(permDict, "plans:delete", "plans");
        SetParent(permDict, "orders:edit", "orders");
        SetParent(permDict, "orders:view", "orders");
        SetParent(permDict, "tags:add", "tags");
        SetParent(permDict, "tags:edit", "tags");
        SetParent(permDict, "tags:delete", "tags");
        SetParent(permDict, "users:add", "users");
        SetParent(permDict, "users:edit", "users");

        SetParent(permDict, "role", "system");
        SetParent(permDict, "permission", "system");
        SetParent(permDict, "dataDictionary", "system");

        SetParent(permDict, "role:add", "role");
        SetParent(permDict, "role:edit", "role");
        SetParent(permDict, "role:delete", "role");
        SetParent(permDict, "permission:add", "permission");
        SetParent(permDict, "permission:edit", "permission");
        SetParent(permDict, "permission:delete", "permission");
        SetParent(permDict, "dataDictionary:add", "dataDictionary");
        SetParent(permDict, "dataDictionary:edit", "dataDictionary");
        SetParent(permDict, "dataDictionary:delete", "dataDictionary");

        dbContext.SaveChanges();

        var allPermCodes = permList.Where(p => p.Code != "root").Select(p => p.Code).ToList();
        var adminRole = roles.First(r => r.Code == "admin");
        var merchantRole = roles.First(r => r.Code == "merchant");
        var userRole = roles.First(r => r.Code == "user");

        foreach (var code in allPermCodes)
        {
            if (permDict.TryGetValue(code, out var perm))
                dbContext.SystemRolePermissions.Add(new SystemRolePermission(adminRole.Id, perm.Id));
        }

        var merchantPerms = new[] {
            "home", "shops", "shops:add", "shops:edit", "shops:delete",
            "rooms", "rooms:add", "rooms:edit",
            "orders", "orders:edit", "orders:view",
            "coupons", "coupons:add", "coupons:edit",
            "plans", "plans:edit",
            "tags", "tags:add", "tags:edit", "tags:delete"
        };
        foreach (var code in merchantPerms)
        {
            if (permDict.TryGetValue(code, out var perm))
                dbContext.SystemRolePermissions.Add(new SystemRolePermission(merchantRole.Id, perm.Id));
        }

        var userPerms = new[] { "home", "orders", "orders:view" };
        foreach (var code in userPerms)
        {
            if (permDict.TryGetValue(code, out var perm))
                dbContext.SystemRolePermissions.Add(new SystemRolePermission(userRole.Id, perm.Id));
        }

        dbContext.SaveChanges();
    }

    private static void SetParent(Dictionary<string, SystemPermission> dict, string childCode, string parentCode)
    {
        if (dict.TryGetValue(childCode, out var child) && dict.TryGetValue(parentCode, out var parent))
        {
            child.GetType().GetProperty("ParentId")?.SetValue(child, parent.Id);
        }
    }

    private static void InitializeReviews(AppDbContext dbContext)
    {
        if (dbContext.Reviews.Any())
            return;

        var completedOrders = dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed)
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

        var ordersForReview = completedOrders.OrderBy(o => Guid.NewGuid()).Take(30).ToList();

        foreach (var order in ordersForReview)
        {
            var score = random.Next(1, 6);
            var content = comments[random.Next(comments.Length)];

            var review = new Review(order.Id, order.RoomId, order.CustomerId.Value, score, content);
            reviews.Add(review);

            var room = dbContext.Rooms.FirstOrDefault(r => r.Id == order.RoomId);
            if (room != null)
            {
                room.AddRating((decimal)score);
            }
        }

        dbContext.Reviews.AddRange(reviews);
        dbContext.SaveChanges();
    }

    private static void InitializeDataDictionaries(AppDbContext dbContext)
    {
        if (dbContext.SystemDataDictionaries.Any())
            return;

        var dictionaries = new List<SystemDataDictionary>
        {
            new SystemDataDictionary(Guid.NewGuid(), "room_status", "房间状态", "Idle", "房间状态枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "room_status_occupied", "占用", "Occupied", "房间已被占用", 2, true),
            new SystemDataDictionary(Guid.NewGuid(), "room_status_cleaning", "清洁中", "Cleaning", "房间清洁中", 3, true),
            new SystemDataDictionary(Guid.NewGuid(), "room_status_fault", "故障", "Fault", "房间故障", 4, true),

            new SystemDataDictionary(Guid.NewGuid(), "order_status", "订单状态", "Pending", "订单状态枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "order_status_started", "进行中", "Started", "订单进行中", 2, true),
            new SystemDataDictionary(Guid.NewGuid(), "order_status_completed", "已完成", "Completed", "订单已完成", 3, true),
            new SystemDataDictionary(Guid.NewGuid(), "order_status_canceled", "已取消", "Canceled", "订单已取消", 4, true),

            new SystemDataDictionary(Guid.NewGuid(), "coupon_type", "优惠券类型", "fixed", "优惠券类型枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "coupon_type_percent", "折扣券", "percent", "按比例折扣", 2, true),

            new SystemDataDictionary(Guid.NewGuid(), "pay_method", "支付方式", "wechat", "支付方式枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "pay_method_alipay", "支付宝", "alipay", "支付宝支付", 2, true),
            new SystemDataDictionary(Guid.NewGuid(), "pay_method_cash", "现金", "cash", "现金支付", 3, true),

            new SystemDataDictionary(Guid.NewGuid(), "customer_level", "客户等级", "normal", "客户等级枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "customer_level_vip", "VIP", "vip", "VIP客户", 2, true),
            new SystemDataDictionary(Guid.NewGuid(), "customer_level_vip2", "VIP2", "vip2", "高级VIP客户", 3, true),

            new SystemDataDictionary(Guid.NewGuid(), "shop_status", "门店状态", "normal", "门店状态枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "shop_status_closed", "休息", "closed", "门店休息中", 2, true),
            new SystemDataDictionary(Guid.NewGuid(), "shop_status_maintenance", "维护", "maintenance", "门店维护中", 3, true),

            new SystemDataDictionary(Guid.NewGuid(), "review_score", "评价星级", "1", "评价星级枚举", 1, true),
            new SystemDataDictionary(Guid.NewGuid(), "review_score_2", "2星", "2", "二星评价", 2, true),
            new SystemDataDictionary(Guid.NewGuid(), "review_score_3", "3星", "3", "三星评价", 3, true),
            new SystemDataDictionary(Guid.NewGuid(), "review_score_4", "4星", "4", "四星评价", 4, true),
            new SystemDataDictionary(Guid.NewGuid(), "review_score_5", "5星", "5", "五星评价", 5, true)
        };

        dbContext.SystemDataDictionaries.AddRange(dictionaries);
        dbContext.SaveChanges();

        var dictDict = dbContext.SystemDataDictionaries.ToDictionary(d => d.Code);

        SetDataDictParent(dictDict, "room_status_occupied", "room_status");
        SetDataDictParent(dictDict, "room_status_cleaning", "room_status");
        SetDataDictParent(dictDict, "room_status_fault", "room_status");

        SetDataDictParent(dictDict, "order_status_started", "order_status");
        SetDataDictParent(dictDict, "order_status_completed", "order_status");
        SetDataDictParent(dictDict, "order_status_canceled", "order_status");

        SetDataDictParent(dictDict, "coupon_type_percent", "coupon_type");

        SetDataDictParent(dictDict, "pay_method_alipay", "pay_method");
        SetDataDictParent(dictDict, "pay_method_cash", "pay_method");

        SetDataDictParent(dictDict, "customer_level_vip", "customer_level");
        SetDataDictParent(dictDict, "customer_level_vip2", "customer_level");

        SetDataDictParent(dictDict, "shop_status_closed", "shop_status");
        SetDataDictParent(dictDict, "shop_status_maintenance", "shop_status");

        SetDataDictParent(dictDict, "review_score_2", "review_score");
        SetDataDictParent(dictDict, "review_score_3", "review_score");
        SetDataDictParent(dictDict, "review_score_4", "review_score");
        SetDataDictParent(dictDict, "review_score_5", "review_score");

        dbContext.SaveChanges();
    }

    private static void SetDataDictParent(Dictionary<string, SystemDataDictionary> dict, string childCode, string parentCode)
    {
        if (dict.TryGetValue(childCode, out var child) && dict.TryGetValue(parentCode, out var parent))
        {
            child.GetType().GetProperty("ParentId")?.SetValue(child, parent.Id);
        }
    }
}
