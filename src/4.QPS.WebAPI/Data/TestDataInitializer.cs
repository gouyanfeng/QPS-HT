using QPS.Domain.Aggregates.MerchantAggregate;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.RoomAggregate;
using QPS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        // Add test data if database is empty
        if (!dbContext.Merchants.Any())
        {
            // 创建多个商户
            var merchants = new List<Merchant>();
            for (int i = 1; i <= 5; i++)
            {
                var merchant = new Merchant(
                    $"商户{i}",
                    $"1380013800{i}",
                    new StoreSettings(30, new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0))
                );
                merchants.Add(merchant);
                dbContext.Merchants.Add(merchant);
            }

            // 为每个商户创建多个房间
            var rooms = new List<Room>();
            foreach (var merchant in merchants)
            {
                for (int i = 1; i <= 10; i++)
                {
                    var room = new Room(
                        $"{merchant.Name.Substring(2)}{i:00}",
                        new DeviceConfig($"room/{merchant.Id}/{i}", "ON", "OFF"),
                        merchant.Id
                    );
                    rooms.Add(room);
                    dbContext.Rooms.Add(room);
                }
            }

            // 创建1000个订单
            var orders = new List<Order>();
            var random = new Random();
            for (int i = 1; i <= 1000; i++)
            {
                // 随机选择一个商户
                var merchant = merchants[random.Next(merchants.Count)];
                
                // 随机选择该商户的一个房间
                var merchantRooms = rooms.Where(r => r.MerchantId == merchant.Id).ToList();
                var room = merchantRooms[random.Next(merchantRooms.Count)];
                
                // 生成随机金额
                var amount = random.Next(20, 200);
                
                // 创建订单
                var order = new Order(
                    $"ORD{DateTime.Now:yyyyMMddHHmmss}{i:0000}",
                    room.Id,
                    merchant.Id,
                    new PricingStrategy(10, 8, 2, 100)
                );
                order.Pay(amount);
                orders.Add(order);
                dbContext.Orders.Add(order);
            }

            dbContext.SaveChanges();
        }
    }
}