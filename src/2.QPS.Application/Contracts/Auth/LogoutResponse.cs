namespace QPS.Application.Contracts.Auth;

/// <summary>
/// 登出响应
/// </summary>
public class LogoutResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; }
}