using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using QPS.Application.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace QPS.Infrastructure.IoT;

public class MqttClientService : IMqttService
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttClientService> _logger;
    private bool _isConnected;

    public MqttClientService(ILogger<MqttClientService> logger)
    {
        _logger = logger;
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        _isConnected = false;
    }

    private async Task ConnectIfNeededAsync()
    {
        if (!_isConnected)
        {
            try
            {
                // 这里使用默认的 MQTT 服务器地址，实际项目中应该从配置中读取
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost", 1883)
                    .WithClientId(Guid.NewGuid().ToString())
                    .Build();

                await _mqttClient.ConnectAsync(options);
                _isConnected = true;
            }
            catch (Exception ex)
            {
                // 记录错误，但不抛出异常，避免影响主要功能
                _logger.LogWarning(ex, "MQTT 连接失败");
                _isConnected = false;
            }
        }
    }

    public async Task SendCommandAsync(string topic, string command)
    {
        try
        {
            await ConnectIfNeededAsync();
            
            if (_isConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(command)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
            else
            {
                // MQTT 未连接，记录警告但继续执行
                _logger.LogWarning("MQTT 未连接，跳过发送命令");
            }
        }
        catch (Exception ex)
        {
            // 记录错误，但不抛出异常，避免影响主要功能
                _logger.LogError(ex, "发送 MQTT 命令失败");
        }
    }

    public async Task SubscribeAsync(string topic, Func<string, Task> handler)
    {
        try
        {
            await ConnectIfNeededAsync();
            
            if (_isConnected)
            {
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
                _mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    if (e.ApplicationMessage.Topic == topic)
                    {
                        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                        await handler(payload);
                    }
                };
            }
        }
        catch (Exception ex)
        {
            // 记录错误，但不抛出异常，避免影响主要功能
            _logger.LogError(ex, "订阅 MQTT 主题失败");
        }
    }
}