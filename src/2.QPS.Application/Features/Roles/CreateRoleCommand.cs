using MediatR;
using QPS.Application.Contracts.Roles;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Roles;

/// <summary>
/// 创建角色命令
/// </summary>
public class CreateRoleCommand : IRequest<RoleDto>
{
    /// <summary>
    /// 创建角色请求
    /// </summary>
    public RoleCreateRequest Request { get; set; }
}

/// <summary>
/// 创建角色处理器
/// </summary>
public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="currentUserService">当前用户服务</param>
    public CreateRoleHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 处理创建角色请求
    /// </summary>
    /// <param name="request">创建角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色DTO</returns>
    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // 从当前用户服务获取当前商户ID
        var merchantId = _currentUserService.MerchantId;

        // 创建角色
        var role = new Role(merchantId, request.Request.Name, request.Request.Code);

        // 保存到数据库
        _dbContext.Roles.Add(role);
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