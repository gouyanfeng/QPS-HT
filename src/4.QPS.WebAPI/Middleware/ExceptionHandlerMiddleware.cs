using Microsoft.AspNetCore.Http;
using QPS.Application.Features.System;
using QPS.Domain.Exceptions;
using System.Text;
using System.Text.Json;

namespace QPS.WebAPI.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogService errorLogService)
    {
        try
        {
            // 启用请求体缓冲，允许异常时多次读取 Body
            context.Request.EnableBuffering();

            await _next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught: {ex.GetType().Name} - {ex.Message}");
            await HandleExceptionAsync(context, ex, errorLogService);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, IErrorLogService errorLogService)
    {
        try
        {
            string requestUrl = context.Request.Path + context.Request.QueryString;
            string requestMethod = context.Request.Method;
            string requestBody = string.Empty;

            try
            {
                // Body 已在 InvokeAsync 中启用缓冲，可以重置位置重复读取
                if (context.Request.Body.CanSeek)
                {
                    context.Request.Body.Position = 0;
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                requestBody = $"Error reading body: {ex.Message}";
            }

            string userId = context.User?.FindFirst("userId")?.Value ?? string.Empty;
            string username = context.User?.FindFirst("username")?.Value ?? string.Empty;
            string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            string userAgent = context.Request.Headers["User-Agent"].ToString();

            int httpStatusCode = exception is BusinessException bizEx ? bizEx.ErrorCode : 500;

            try
            {
                await errorLogService.LogErrorAsync(
                    exception,
                    requestUrl,
                    requestMethod,
                    requestBody,
                    userId,
                    username,
                    ipAddress,
                    userAgent,
                    httpStatusCode
                );
                Console.WriteLine("Error log saved successfully");
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error saving log: {logEx.Message}");
            }

            // 统一返回 200，错误信息在 JSON body 中通过 code 字段传递
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            int errorCode = httpStatusCode;
            string message = exception.Message;

            var response = QPS.Application.Common.ApiResponse<object>.Fail(errorCode, message);

            await context.Response.WriteAsJsonAsync(response, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error in exception handler: {ex.Message}");
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"code\":500,\"msg\":\"Critical error\"}");
        }
    }
}