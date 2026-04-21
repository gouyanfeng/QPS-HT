using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class DeviceConfig : ValueObject
{
    public string MqttTopic { get; private set; }
    public string PowerOnCommand { get; private set; }
    public string PowerOffCommand { get; private set; }

    protected DeviceConfig() { }

    public DeviceConfig(string mqttTopic, string powerOnCommand, string powerOffCommand)
    {
        MqttTopic = mqttTopic;
        PowerOnCommand = powerOnCommand;
        PowerOffCommand = powerOffCommand;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MqttTopic;
        yield return PowerOnCommand;
        yield return PowerOffCommand;
    }
}