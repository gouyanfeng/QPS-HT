using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Users;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;
using System;

namespace QPS.Application.Features.Users;

/// <summary>
/// 获取用户详情查询
/// </summary>
public class GetUserQuery : IRequest<UserDto>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 获取用户详情处理器
/// </summary>
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public GetUserHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理获取用户详情请求
    /// </summary>
    /// <param name="request">获取用户详情查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户DTO</returns>
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // 查询用户
        var user = await _dbContext.SystemUsers.FindAsync(new object[] { request.Id }, cancellationToken);

        if (user == null)
        {
            throw new BusinessException(404, "用户不存在");
        }

        // 从关联表查询角色ID
        var userRole = await _dbContext.SystemUserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.Id, cancellationToken);

        // 转换为DTO
        return new UserDto
        {
            Id = user.Id,
            MerchantId = user.MerchantId,
            RoleId = userRole?.RoleId ?? Guid.Empty,
            Username = user.Username,
            RealName = user.RealName,
            IsActive = user.IsActive
        };
    }
}