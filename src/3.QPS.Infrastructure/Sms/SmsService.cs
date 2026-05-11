using Microsoft.Extensions.Logging;

namespace QPS.Infrastructure.Sms;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message);
}

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        // 模拟短信发送
        _logger.LogInformation($"发送短信到 {phoneNumber}: {message}");

        // 实际项目中这里会调用短信服务商的API
        // 例如阿里云短信、腾讯云短信等

        await Task.Delay(100); // 模拟网络延迟
    }
}