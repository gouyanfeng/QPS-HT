namespace QPS.Application.Contracts.Permissions;

/// <summary>
/// 角色权限映射 DTO
/// </summary>
public class RolePermissionsDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 权限代码列表
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}
