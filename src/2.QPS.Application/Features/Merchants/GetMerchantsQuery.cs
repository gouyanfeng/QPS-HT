using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using System.Collections.Generic;
using System.Linq;

namespace QPS.Application.Features.Merchants;

/// <summary>
/// 获取商户列表查询
/// </summary>
public class GetMerchantsQuery : PaginationRequest, IRequest<PaginationResponse<MerchantDto>>
{
    /// <summary>
    /// 商户名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 商户电话
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 获取商户列表处理器
/// </summary>
public class GetMerchantsHandler : IRequestHandler<GetMerchantsQuery, PaginationResponse<MerchantDto>>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public GetMerchantsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理获取商户列表请求
    /// </summary>
    /// <param name="request">获取商户列表查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商户DTO分页响应</returns>
    public async Task<PaginationResponse<MerchantDto>> Handle(GetMerchantsQuery request, CancellationToken cancellationToken)
    {
        // 构建查询
        var query = _dbContext.Merchants.AsQueryable();

        // 应用查询条件
        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(m => m.Name.Contains(request.Name));
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            query = query.Where(m => m.Phone.Contains(request.Phone));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(m => m.IsActive == request.IsActive.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(m => m.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(m => m.CreatedAt <= request.EndDate.Value);
        }

        // 转换为DTO
        var dtoQuery = query.Select(m => new MerchantDto
        {
            Id = m.Id,
            Name = m.Name,
            Phone = m.Phone,
            ExpiryDate = m.ExpiryDate,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        });

        // 执行分页查询
        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}