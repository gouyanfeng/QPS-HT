using MediatR;
using QPS.Application.Contracts.Roles;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Roles;

/// <summary>
/// 更新角色命令
/// </summary>
public class UpdateRoleCommand : IRequest<RoleDto>
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 更新角色请求
    /// </summary>
    public RoleUpdateRequest Request { get; set; }
}

/// <summary>
/// 更新角色处理器
/// </summary>
public class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public UpdateRoleHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理更新角色请求
    /// </summary>
    /// <param name="request">更新角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色DTO</returns>
    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // 查询角色
        var role = await _dbContext.Roles.FindAsync(request.Id, cancellationToken);

        if (role == null)
        {
            throw new BusinessException(404, "角色不存在");
        }

        // 更新角色信息
        role.Update(request.Request.Name, request.Request.Code);

        // 保存到数据库
        await _dbContext.SaveChangesAsync(cancellationToken);

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