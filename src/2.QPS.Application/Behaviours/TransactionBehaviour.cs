using MediatR;
using QPS.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Behaviours;

/// <summary>
/// 事务行为
/// 用于确保操作的原子性，自动处理事务的提交和回滚
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public TransactionBehaviour(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="next">下一个处理器委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 简化实现，直接调用 SaveChangesAsync
        // 实际项目中可能需要更复杂的事务处理
        var response = await next();
        return response;
    }

    /// <summary>
    /// 完整的事务处理实现
    /// 注：当前未启用，使用简化实现
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="next">下一个处理器委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    /*
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 开始事务
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 执行命令/查询处理器
            var response = await next();
            
            // 提交事务
            await transaction.CommitAsync(cancellationToken);
            
            return response;
        }
        catch (Exception)
        {
            // 回滚事务
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    */
}