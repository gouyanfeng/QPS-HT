namespace QPS.Application.Contracts.System.Permissions;

/// <summary>
/// 更新角色权限请求
/// </summary>
public class UpdateRolePermissionsRequest
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
