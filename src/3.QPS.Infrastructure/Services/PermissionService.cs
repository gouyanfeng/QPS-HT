using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QPS.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public PermissionService(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return false;

        var permissions = await GetUserPermissionsAsync();

        if (permissions.Contains(permissionCode))
            return true;

        var wildcardPatterns = permissions.Where(p => p.EndsWith(":*")).ToList();
        foreach (var pattern in wildcardPatterns)
        {
            var prefix = pattern.Substring(0, pattern.Length - 2);
            if (permissionCode.StartsWith(prefix))
                return true;
        }

        return false;
    }

    public async Task<List<string>> GetUserPermissionsAsync()
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return new List<string>();

        var permissions = await _dbContext.SystemUserRoles
            .Where(ur => ur.UserId == Guid.Parse(userId))
            .Join(
                _dbContext.SystemRolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionId
            )
            .Join(
                _dbContext.SystemPermissions,
                pid => pid,
                p => p.Id,
                (pid, p) => p.PermissionCode
            )
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> HasAnyPermissionAsync(params string[] permissionCodes)
    {
        var permissions = await GetUserPermissionsAsync();

        foreach (var code in permissionCodes)
        {
            if (permissions.Contains(code))
                return true;

            var wildcardPatterns = permissions.Where(p => p.EndsWith(":*")).ToList();
            foreach (var pattern in wildcardPatterns)
            {
                var prefix = pattern.Substring(0, pattern.Length - 2);
                if (code.StartsWith(prefix))
                    return true;
            }
        }

        return false;
    }

    public async Task<bool> HasAllPermissionsAsync(params string[] permissionCodes)
    {
        var permissions = await GetUserPermissionsAsync();

        foreach (var code in permissionCodes)
        {
            var hasPermission = permissions.Contains(code);

            if (!hasPermission)
            {
                var wildcardPatterns = permissions.Where(p => p.EndsWith(":*")).ToList();
                hasPermission = wildcardPatterns.Any(p =>
                    code.StartsWith(p.Substring(0, p.Length - 2)));
            }

            if (!hasPermission)
                return false;
        }

        return true;
    }
}