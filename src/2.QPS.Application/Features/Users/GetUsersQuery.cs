using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Users;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using System.Collections.Generic;
using System.Linq;

namespace QPS.Application.Features.Users;

/// <summary>
/// 获取用户列表查询
/// </summary>
public class GetUsersQuery : PaginationRequest, IRequest<PaginationResponse<UserDto>>
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string? RealName { get; set; }

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
/// 获取用户列表处理器
/// </summary>
public class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginationResponse<UserDto>>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public GetUsersHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理获取用户列表请求
    /// </summary>
    /// <param name="request">获取用户列表查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户DTO分页响应</returns>
    public async Task<PaginationResponse<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // 构建查询，全局查询过滤器会自动过滤MerchantId
        var query = _dbContext.Users.AsQueryable();

        // 应用查询条件
        if (!string.IsNullOrEmpty(request.Username))
        {
            query = query.Where(u => u.Username.Contains(request.Username));
        }

        if (!string.IsNullOrEmpty(request.RealName))
        {
            query = query.Where(u => u.RealName.Contains(request.RealName));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= request.EndDate.Value);
        }

        // 转换为DTO
        var dtoQuery = query.Select(u => new UserDto
        {
            Id = u.Id,
            MerchantId = u.MerchantId,
            Username = u.Username,
            RealName = u.RealName,
            IsActive = u.IsActive
        });

        // 执行分页查询
        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}