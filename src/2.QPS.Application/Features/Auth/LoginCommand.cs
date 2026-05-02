using MediatR;
using QPS.Application.Contracts.Auth;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Auth;

/// <summary>
/// 登录命令
/// </summary>
public class LoginCommand : IRequest<LoginResponse>
{
    /// <summary>
    /// 登录请求
    /// </summary>
    public LoginRequest Request { get; set; }
}

/// <summary>
/// 登录处理器
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IDbContext _dbContext;
    private readonly IJwtGenerator _jwtGenerator;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="jwtGenerator">JWT生成器</param>
    public LoginHandler(IDbContext dbContext, IJwtGenerator jwtGenerator)
    {
        _dbContext = dbContext;
        _jwtGenerator = jwtGenerator;
    }

    /// <summary>
    /// 处理登录请求
    /// </summary>
    /// <param name="request">登录命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录响应</returns>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 验证用户名和密码
        var user = await ValidateUserAsync(
            request.Request.Username,
            request.Request.Password,
            cancellationToken
        );

        // 生成JWT令牌
        var token = _jwtGenerator.GenerateToken(user.Id, user.MerchantId, "Admin");

        // 返回登录响应
        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            RealName = user.RealName,
            Role = "Admin",
            MerchantId = user.MerchantId,
            RoleId = user.RoleId
        };
    }

    /// <summary>
    /// 验证用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证通过的用户</returns>
    private async Task<User> ValidateUserAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.IgnoreQueryFilters()
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            throw new BusinessException(401, "用户名或密码错误");
        }

        if (!user.IsActive)
        {
            throw new BusinessException(403, "用户已被禁用");
        }

        return user;
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="passwordHash">密码哈希</param>
    /// <returns>验证结果</returns>
    private bool VerifyPassword(string password, string passwordHash)
    {
        // 这里应该使用密码哈希验证，例如使用BCrypt
        // 为了测试方便，这里简单比较密码和哈希值
        return password == passwordHash;
    }
}