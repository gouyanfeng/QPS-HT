using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.Auth;

public class ChangePasswordCommand : IRequest<bool>
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            throw new BusinessException(401, "用户未登录");

        var user = await _dbContext.SystemUsers.FindAsync(new object[] { userGuid }, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new BusinessException(404, "用户不存在");

        // 验证旧密码
        if (user.PasswordHash != request.OldPassword)
            throw new BusinessException(400, "旧密码错误");

        // 更新密码
        user.ChangePassword(request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
