namespace QPS.Application.Common;

/// <summary>
/// API 响应包装类
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 响应数据
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// 响应代码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Msg { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <returns>API 响应</returns>
    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T> { Data = data, Code = 200, Msg = "操作成功" };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    /// <param name="code">响应代码</param>
    /// <param name="msg">响应消息</param>
    /// <returns>API 响应</returns>
    public static ApiResponse<T> Fail(int code, string msg)
    {
        return new ApiResponse<T> { Data = default, Code = code, Msg = msg };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    /// <param name="msg">响应消息</param>
    /// <returns>API 响应</returns>
    public static ApiResponse<T> Fail(string msg)
    {
        return Fail(500, msg);
    }
}