using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Permissions;

/// <summary>
/// 更新角色权限
/// </summary>
public class UpdateRolePermissionsCommand : IRequest<bool>
{
    /// <summary>
    /// 角色代码
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 权限代码列表
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}

public class UpdateRolePermissionsHandler : IRequestHandler<UpdateRolePermissionsCommand, bool>
{
    private readonly IDbContext _dbContext;

    public UpdateRolePermissionsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        // 查找角色（绕过租户过滤，权限管理为系统级操作）
        var role = await _dbContext.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Code == request.Role && !r.IsDeleted, cancellationToken);

        if (role == null)
            throw new BusinessException(404, "角色不存在");

        // 删除该角色所有旧的权限
        var existing = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == role.Id && !rp.IsDeleted)
            .ToListAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(existing);

        // 查询权限 ID 映射
        var permCodes = request.Permissions.Distinct().ToList();
        var permIdMap = await _dbContext.Permissions
            .Where(p => permCodes.Contains(p.Code) && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Code, p => p.Id, cancellationToken);

        // 插入新的权限（使用 PermissionId）
        foreach (var permCode in permCodes)
        {
            if (permIdMap.TryGetValue(permCode, out var permId))
            {
                var rp = new RolePermission(role.Id, permId);
                _dbContext.RolePermissions.Add(rp);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
