using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using QPS.Application.Common;

namespace QPS.WebAPI.Filters;

/// <summary>
/// 响应包装过滤器
/// </summary>
public class ResponseWrapperFilter : IAsyncResultFilter
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // 默认 data 为 null
        object? data = null;

        // 使用模式匹配：如果是 ObjectResult，直接取值给 data
        if (context.Result is ObjectResult objectResult)
        {
            data = objectResult.Value;
        }
        // 如果是 EmptyResult 或 StatusCodeResult (如 NoContent)，data 保持为 null




        Console.WriteLine($"OnResultExecutionAsync: {data}");

        var response = ApiResponse<object>.Success(data);

        context.Result = new JsonResult(response, _jsonOptions)
        {
            StatusCode = 200
        };

        await next();
    }
}