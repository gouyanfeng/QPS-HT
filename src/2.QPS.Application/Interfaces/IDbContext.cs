using Microsoft.EntityFrameworkCore;
using QPS.Domain.Entities;
using QPS.Domain.Entities;
using QPS.Domain.Entities;
using QPS.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace QPS.Application.Interfaces;

public interface IDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Order> Orders { get; }
    DbSet<Merchant> Merchants { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}