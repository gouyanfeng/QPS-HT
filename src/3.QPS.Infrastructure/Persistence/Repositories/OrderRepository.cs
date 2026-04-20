using Microsoft.EntityFrameworkCore;
using QPS.Domain.Aggregates.OrderAggregate;

namespace QPS.Infrastructure.Persistence.Repositories;

public class OrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _dbContext.Orders.FindAsync(id);
    }

    public async Task<List<Order>> GetByMerchantIdAsync(Guid merchantId)
    {
        return await _dbContext.Orders.Where(o => o.MerchantId == merchantId).ToListAsync();
    }

    public void Add(Order order)
    {
        _dbContext.Orders.Add(order);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
    }
}