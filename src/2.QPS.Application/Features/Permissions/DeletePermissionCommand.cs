using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Permissions;

public class DeletePermissionCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePermissionHandler : IRequestHandler<DeletePermissionCommand>
{
    private readonly IDbContext _dbContext;

    public DeletePermissionHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _dbContext.SystemPermissions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (permission == null)
        {
            throw new BusinessException(404, "权限不存在");
        }

        // 删除关联的角色-权限关系
        var rolePermissions = _dbContext.SystemRolePermissions
            .Where(rp => rp.PermissionId == request.Id);

        _dbContext.SystemRolePermissions.RemoveRange(rolePermissions);

        // 删除权限
        _dbContext.SystemPermissions.Remove(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
