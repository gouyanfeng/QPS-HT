namespace QPS.Application.Interfaces;

/// <summary>
/// 短信服务接口
/// </summary>
public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message);
}
