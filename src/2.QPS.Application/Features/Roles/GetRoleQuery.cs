using MediatR;
using QPS.Application.Contracts.Roles;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Roles;

/// <summary>
/// 获取角色详情查询
/// </summary>
public class GetRoleQuery : IRequest<RoleDto>
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 获取角色详情处理器
/// </summary>
public class GetRoleHandler : IRequestHandler<GetRoleQuery, RoleDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public GetRoleHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理获取角色详情请求
    /// </summary>
    /// <param name="request">获取角色详情查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色DTO</returns>
    public async Task<RoleDto> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        // 查询角色
        var role = await _dbContext.Roles.FindAsync(request.Id, cancellationToken);

        if (role == null)
        {
            throw new BusinessException(404, "角色不存在");
        }

        // 转换为DTO
        return new RoleDto
        {
            Id = role.Id,
            MerchantId = role.MerchantId,
            Name = role.Name,
            Code = role.Code
        };
    }
}