namespace QPS.Application.Interfaces;

public interface IMqttService
{
    Task SendCommandAsync(string topic, string command);
    Task SubscribeAsync(string topic, Func<string, Task> handler);
}