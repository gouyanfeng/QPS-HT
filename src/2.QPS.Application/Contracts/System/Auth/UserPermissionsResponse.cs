namespace QPS.Application.Contracts.System.Auth;

/// <summary>
/// 当前用户权限响应
/// </summary>
public class UserPermissionsResponse
{
    /// <summary>
    /// 权限代码列表（如 ["HOME", "CRM_HERB_BASE_ADD", "DATA_DICTIONARY_EDIT"]）
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}


