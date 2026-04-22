using MediatR;
using QPS.Application.Contracts.Auth;
using QPS.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Auth;

/// <summary>
/// 登出命令
/// </summary>
public class LogoutCommand : IRequest<LogoutResponse>
{
    /// <summary>
    /// 登出请求
    /// </summary>
    public LogoutRequest Request { get; set; }
}

/// <summary>
/// 登出处理器
/// </summary>
public class LogoutHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUserService">当前用户服务</param>
    public LogoutHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 处理登出请求
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登出响应</returns>
    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 这里可以添加登出逻辑，例如：
        // 1. 将 token 添加到黑名单
        // 2. 清理用户会话
        // 3. 记录登出日志

        // 由于 JWT 是无状态的，我们可以简单地返回成功响应
        // 客户端收到响应后，应该删除本地存储的 token

        return new LogoutResponse
        {
            Success = true,
            Message = "登出成功"
        };
    }
}