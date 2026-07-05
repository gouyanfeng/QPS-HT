using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Permissions;

/// <summary>
/// 获取所有角色的权限映射
/// </summary>
public class GetRolePermissionsQuery : IRequest<Dictionary<string, RolePermissionsDto>>
{
    /// <summary>
    /// 可选的角色代码筛选
    /// </summary>
    public string? RoleCode { get; set; }
}

public class GetRolePermissionsHandler : IRequestHandler<GetRolePermissionsQuery, Dictionary<string, RolePermissionsDto>>
{
    private readonly IDbContext _dbContext;

    public GetRolePermissionsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<string, RolePermissionsDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var rolesQuery = _dbContext.Roles
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.RoleCode))
        {
            rolesQuery = rolesQuery.Where(r => r.Code == request.RoleCode);
        }

        var roles = await rolesQuery.ToListAsync(cancellationToken);

        // 获取所有角色的权限映射和权限字典
        var rolePermissions = await _dbContext.RolePermissions
            .Where(rp => !rp.IsDeleted)
            .ToListAsync(cancellationToken);

        var permissionMap = await _dbContext.Permissions
            .Where(p => !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, p => p.Code, cancellationToken);

        var result = new Dictionary<string, RolePermissionsDto>();

        foreach (var role in roles)
        {
            var permissions = rolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => permissionMap.GetValueOrDefault(rp.PermissionId, string.Empty))
                .Where(code => !string.IsNullOrEmpty(code))
                .OrderBy(x => x)
                .ToList();

            result[role.Code] = new RolePermissionsDto
            {
                RoleName = role.Name,
                Permissions = permissions
            };
        }

        return result;
    }
}
