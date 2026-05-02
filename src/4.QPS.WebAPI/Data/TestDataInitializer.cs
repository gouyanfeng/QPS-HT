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
        var tags = InitializeTags(dbContext, merchant);
        var rooms = InitializeRooms(dbContext, shops, merchant);
        InitializeRoomImages(dbContext, rooms, merchant);
        var plans = InitializePlans(dbContext, merchant);
        InitializeRoomTags(dbContext, rooms, tags, merchant);
        InitializeRoomPlans(dbContext, rooms, plans, merchant);
        InitializeCoupons(dbContext, merchant);
        var customers = InitializeCustomers(dbContext, merchant);
        InitializeOrders(dbContext, shops, rooms, customers, merchant);
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
            Shop.Create("旗舰店", "北京市朝阳区xxx路1号", new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 30),
            Shop.Create("分店A", "北京市海淀区xxx路2号", new TimeSpan(10, 0, 0), new TimeSpan(21, 0, 0), 20),
            Shop.Create("分店B", "北京市西城区xxx路3号", new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0), 45)
        };

        foreach (var shop in shops)
        {
            shop.GetType().GetProperty("MerchantId")?.SetValue(shop, merchant.Id);
        }

        dbContext.Shops.AddRange(shops);
        dbContext.SaveChanges();
        return shops;
    }

    private static List<Role> InitializeRoles(AppDbContext dbContext, Merchant merchant)
    {
        var existingRoles = dbContext.Roles.IgnoreQueryFilters().Where(r => r.MerchantId == merchant.Id).ToList();
        if (existingRoles.Any())
            return existingRoles;

        var roles = new List<Role>
        {
            new Role("管理员", "admin"),
            new Role("店长", "shop_manager"),
            new Role("收银员", "cashier")
        };

        foreach (var role in roles)
        {
            role.GetType().GetProperty("MerchantId")?.SetValue(role, merchant.Id);
        }

        dbContext.Roles.AddRange(roles);
        dbContext.SaveChanges();
        return roles;
    }

    private static void InitializeUsers(AppDbContext dbContext, Merchant merchant, List<Role> roles)
    {
        var existingUsers = dbContext.Users.IgnoreQueryFilters().Where(u => u.MerchantId == merchant.Id).ToList();
        if (existingUsers.Any())
            return;

        var adminRole = roles.First(r => r.Code == "admin");
        var managerRole = roles.First(r => r.Code == "shop_manager");

        var users = new List<User>
        {
            User.Create("admin", "123456", "系统管理员", adminRole.Id),
            User.Create("manager", "123456", "张店长", managerRole.Id),
            User.Create("cashier1", "123456", "李收银员", roles.First(r => r.Code == "cashier").Id)
        };

        foreach (var user in users)
        {
            user.GetType().GetProperty("MerchantId")?.SetValue(user, merchant.Id);
        }

        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var userRoles = new List<UserRole>
        {
            new UserRole(users[0].Id, adminRole.Id),
            new UserRole(users[1].Id, managerRole.Id),
            new UserRole(users[2].Id, roles.First(r => r.Code == "cashier").Id)
        };

        dbContext.UserRoles.AddRange(userRoles);
        dbContext.SaveChanges();
    }

    private static List<Tag> InitializeTags(AppDbContext dbContext, Merchant merchant)
    {
        var existingTags = dbContext.Tags.IgnoreQueryFilters().Where(t => t.MerchantId == merchant.Id).ToList();
        if (existingTags.Any())
            return existingTags;

        var tags = new List<Tag>
        {
            new Tag("VIP"),
            new Tag("特惠"),
            new Tag("热门"),
            new Tag("安静"),
            new Tag("宽敞")
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
        var roomNumbers = new[] { "A001", "A002", "A003", "B001", "B002", "C001", "C002", "C003", "C004", "D001" };

        for (int i = 0; i < roomNumbers.Length; i++)
        {
            var shop = shops[i % shops.Count];
            rooms.Add(Room.Create(shop.Id, roomNumbers[i], 68.00m + i * 10, i % 3 == 0 ? false : true));
        }

        foreach (var room in rooms)
        {
            room.GetType().GetProperty("MerchantId")?.SetValue(room, merchant.Id);
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

        for (int i = 0; i < 10; i++)
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

        var completedOrders = orders.Take(5).ToList();
        foreach (var order in completedOrders)
        {
            var amount = 68.00m + random.Next(0, 5) * 20;
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
}