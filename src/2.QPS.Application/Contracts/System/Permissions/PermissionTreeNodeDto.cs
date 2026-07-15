namespace QPS.Application.Contracts.System.Permissions;

/// <summary>
/// 权限树节点 DTO
/// </summary>
public class PermissionTreeNodeDto
{
    /// <summary>
    /// 节点 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 权限代码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 权限名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 子节点
    /// </summary>
    public List<PermissionTreeNodeDto>? Children { get; set; }
}
