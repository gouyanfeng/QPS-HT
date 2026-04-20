using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using QPS.Domain.Exceptions;

namespace QPS.WebAPI.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Internal server error";

        if (exception is ValidationException validationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage));
        }
        else if (exception is DomainException domainException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = domainException.Message;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message
        }.ToString());
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandler>();
    }
}