using System.Collections.Generic;
using System.Threading.Tasks;

namespace QPS.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permissionCode);

    Task<List<string>> GetUserPermissionsAsync();

    Task<bool> HasAnyPermissionAsync(params string[] permissionCodes);

    Task<bool> HasAllPermissionsAsync(params string[] permissionCodes);
}