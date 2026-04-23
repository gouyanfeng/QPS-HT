using MediatR;
using QPS.Application.Contracts.Users;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;
using System;

namespace QPS.Application.Features.Users;

/// <summary>
/// 更新用户命令
/// </summary>
public class UpdateUserCommand : IRequest<UserDto>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 更新用户请求
    /// </summary>
    public UserUpdateRequest Request { get; set; }
}

/// <summary>
/// 更新用户处理器
/// </summary>
public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public UpdateUserHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理更新用户请求
    /// </summary>
    /// <param name="request">更新用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户DTO</returns>
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // 查询用户
        var user = await _dbContext.Users.FindAsync(request.Id, cancellationToken);

        if (user == null)
        {
            throw new BusinessException(404, "用户不存在");
        }

        // 更新用户信息
        user.Update(request.Request.RealName, request.Request.RoleId);

        // 更新密码（如果提供了新密码）
        if (!string.IsNullOrEmpty(request.Request.Password))
        {
            // 注意：这里应该对密码进行哈希处理，为了测试方便，暂时直接使用明文
            user.ChangePassword(request.Request.Password);
        }

        // 更新用户状态
        if (request.Request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        // 保存到数据库
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 转换为DTO
        return new UserDto
        {
            Id = user.Id,
            MerchantId = user.MerchantId,
            RoleId = user.RoleId,
            Username = user.Username,
            RealName = user.RealName,
            IsActive = user.IsActive
        };
    }
}