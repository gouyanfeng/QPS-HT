using QPS.Domain.Entities;
using QPS.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QPS.WebAPI.Data;

public static class TestDataInitializer
{
    public static void Initialize(AppDbContext dbContext)
    {
        // 初始化管理员用户
        if (!dbContext.Users.Any())
        {
            var adminUser = User.Create("admin", "123456", "Admin");
            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
        }
    }
}