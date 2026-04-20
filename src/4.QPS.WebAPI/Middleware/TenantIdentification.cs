using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QPS.Application.Interfaces;

namespace QPS.WebAPI.Middleware;

public class TenantIdentification
{
    private readonly RequestDelegate _next;

    public TenantIdentification(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITenantService tenantService)
    {
        // 从请求头或Token中获取租户ID
        if (context.Request.Headers.TryGetValue("X-Merchant-Id", out var merchantIdStr) &&
            Guid.TryParse(merchantIdStr, out var merchantId))
        {
            tenantService.SetTenantId(merchantId);
        }

        await _next(context);
    }
}

public static class TenantIdentificationExtensions
{
    public static IApplicationBuilder UseTenantIdentification(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantIdentification>();
    }
}