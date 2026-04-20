using MediatR;
using QPS.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Behaviours;

public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDbContext _dbContext;

    public TransactionBehaviour(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 简化实现，直接调用 SaveChangesAsync
        // 实际项目中可能需要更复杂的事务处理
        var response = await next();
        return response;
    }
}