using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using QPS.Application.Common;
using QPS.Domain.Exceptions;

namespace QPS.WebAPI.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next">下一个中间件</param>
    public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>任务</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="exception">异常</param>
    /// <returns>任务</returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = GetExceptionDetails(exception);
        
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
            ValidationException validationException =>
                (HttpStatusCode.BadRequest, string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage))),
            DomainException domainException =>
                (HttpStatusCode.BadRequest, domainException.Message),
            _ =>
                (HttpStatusCode.InternalServerError, "Internal server error")
        };
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}