using Microsoft.EntityFrameworkCore;
using QPS.Domain.Aggregates.RoomAggregate;

namespace QPS.Infrastructure.Persistence.Repositories;

public class RoomRepository
{
    private readonly AppDbContext _dbContext;

    public RoomRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Room> GetByIdAsync(Guid id)
    {
        return await _dbContext.Rooms.FindAsync(id);
    }

    public async Task<List<Room>> GetByMerchantIdAsync(Guid merchantId)
    {
        return await _dbContext.Rooms.Where(r => r.MerchantId == merchantId).ToListAsync();
    }

    public void Add(Room room)
    {
        _dbContext.Rooms.Add(room);
    }

    public void Update(Room room)
    {
        _dbContext.Rooms.Update(room);
    }
}