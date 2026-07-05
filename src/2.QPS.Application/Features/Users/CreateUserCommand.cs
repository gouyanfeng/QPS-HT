using MediatR;
using QPS.Application.Contracts.Users;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Users;

/// <summary>
/// 创建用户命令
/// </summary>
public class CreateUserCommand : IRequest<UserDto>
{
    /// <summary>
    /// 创建用户请求
    /// </summary>
    public UserCreateRequest Request { get; set; }
}

/// <summary>
/// 创建用户处理器
/// </summary>
public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="currentUserService">当前用户服务</param>
    public CreateUserHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 处理创建用户请求
    /// </summary>
    /// <param name="request">创建用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户DTO</returns>
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 创建用户（注意：这里应该对密码进行哈希处理，为了测试方便，暂时直接使用明文）
        var user = User.Create(
            request.Request.Username,
            request.Request.Password,
            request.Request.RealName,
            request.Request.RoleId
        );

        // 保存到数据库
        _dbContext.Users.Add(user);
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