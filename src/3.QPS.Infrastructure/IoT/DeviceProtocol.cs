namespace QPS.Infrastructure.IoT;

public class DeviceProtocol
{
    public static string ConvertToHex(string command)
    {
        // 简单的十六进制转换逻辑
        return System.BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(command)).Replace("-", "");
    }

    public static string ConvertFromHex(string hex)
    {
        // 从十六进制转换回字符串
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static string CreatePowerOnCommand() => "POWER_ON";
    public static string CreatePowerOffCommand() => "POWER_OFF";
}