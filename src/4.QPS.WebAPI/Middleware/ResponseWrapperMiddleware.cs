using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using QPS.Application.Common;

namespace QPS.WebAPI.Middleware;

/// <summary>
/// 响应包装中间件
/// </summary>
public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next">下一个中间件</param>
    public ResponseWrapperMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>任务</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // 保存原始的响应流
        using var originalBodyStream = context.Response.Body;
        using var responseBody = new System.IO.MemoryStream();
        
        try
        {
            // 捕获响应
            context.Response.Body = responseBody;
            await _next(context);
            
            // 保存原始的状态码
            var originalStatusCode = context.Response.StatusCode;
            
            // 处理响应
            responseBody.Seek(0, System.IO.SeekOrigin.Begin);
            var wrappedContent = await GetWrappedResponseAsync(context, responseBody, originalStatusCode);
            
            // 恢复原始的响应流并写入包装后的响应
            context.Response.Body = originalBodyStream;
            context.Response.StatusCode = 200; // 将 HTTP 状态码设置为 200
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(wrappedContent);
        }
        catch (Exception ex)
        {
            // 处理异常
            await HandleExceptionAsync(context, ex);
        }
        finally
        {
            // 恢复原始的响应流
            context.Response.Body = originalBodyStream;
        }
    }

    /// <summary>
    /// 获取包装后的响应
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="responseBody">响应流</param>
    /// <param name="originalStatusCode">原始状态码</param>
    /// <returns>包装后的响应内容</returns>
    private static async Task<string> GetWrappedResponseAsync(HttpContext context, System.IO.MemoryStream responseBody, int originalStatusCode)
    {
        // 读取响应内容
        var responseContent = await new System.IO.StreamReader(responseBody).ReadToEndAsync();
        
        // 直接包装所有响应，不再检查是否已经是 ApiResponse 格式
        var wrappedResponse = WrapResponse(originalStatusCode, responseContent);
        return JsonSerializer.Serialize(wrappedResponse);
    }

    /// <summary>
    /// 包装响应
    /// </summary>
    /// <param name="statusCode">原始状态码</param>
    /// <param name="responseContent">响应内容</param>
    /// <returns>包装后的响应</returns>
    private static ApiResponse<object> WrapResponse(int statusCode, string responseContent)
    {
        if (statusCode >= 400)
        {
            // 对于失败的操作，data 是 null，msg 是错误内容
            return new ApiResponse<object> { Data = null, Code = statusCode, Msg = string.IsNullOrEmpty(responseContent) ? "操作失败" : responseContent };
        }
        
        if (string.IsNullOrEmpty(responseContent))
        {
            return new ApiResponse<object> { Data = null, Code = statusCode, Msg = "操作成功" };
        }
        
        try
        {
            var data = JsonSerializer.Deserialize<object>(responseContent);
            return new ApiResponse<object> { Data = data, Code = statusCode, Msg = "操作成功" };
        }
        catch
        {
            // 如果响应不是 JSON 格式，直接作为字符串包装
            return new ApiResponse<object> { Data = responseContent, Code = statusCode, Msg = "操作成功" };
        }
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="ex">异常</param>
    /// <returns>任务</returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = GetExceptionDetails(ex);
        
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        
        var errorResponse = ApiResponse<object>.Fail((int)statusCode, message);
        var errorContent = JsonSerializer.Serialize(errorResponse);
        
        await context.Response.WriteAsync(errorContent);
    }

    /// <summary>
    /// 获取异常详情
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>状态码和消息</returns>
    private static (HttpStatusCode, string) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException validationException =>
                (HttpStatusCode.BadRequest, string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage))),
            QPS.Domain.Exceptions.DomainException domainException =>
                (HttpStatusCode.BadRequest, domainException.Message),
            _ =>
                (HttpStatusCode.InternalServerError, "Internal server error")
        };
    }
}