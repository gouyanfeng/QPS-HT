using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.RoomAggregate;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.MerchantAggregate;

namespace QPS.Infrastructure.Persistence;

public class AppDbContext : DbContext, IDbContext
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Merchant> Merchants { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}