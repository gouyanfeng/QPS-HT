using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Roles;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;

namespace QPS.Application.Features.Roles;

/// <summary>
/// 获取角色列表查询
/// </summary>
public class GetRolesQuery : PaginationRequest, IRequest<PaginationResponse<RoleDto>>
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 角色代码
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// 获取角色列表处理器
/// </summary>
public class GetRolesHandler : IRequestHandler<GetRolesQuery, PaginationResponse<RoleDto>>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public GetRolesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理获取角色列表请求
    /// </summary>
    /// <param name="request">获取角色列表查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色DTO分页响应</returns>
    public async Task<PaginationResponse<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        // 构建查询，全局查询过滤器会自动过滤MerchantId
        var query = _dbContext.Roles.AsQueryable();

        // 应用查询条件
        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(r => r.Name.Contains(request.Name));
        }

        if (!string.IsNullOrEmpty(request.Code))
        {
            query = query.Where(r => r.Code.Contains(request.Code));
        }

        // 转换为DTO
        var dtoQuery = query.Select(r => new RoleDto
        {
            Id = r.Id,
            MerchantId = r.MerchantId,
            Name = r.Name,
            Code = r.Code
        });

        // 执行分页查询
        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}