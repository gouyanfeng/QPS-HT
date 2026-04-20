using Microsoft.EntityFrameworkCore;
using QPS.Domain.Aggregates.RoomAggregate;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.MerchantAggregate;
using System.Threading.Tasks;
using System.Threading;

namespace QPS.Application.Interfaces;

public interface IDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Order> Orders { get; }
    DbSet<Merchant> Merchants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}