using QPS.Application.Interfaces;
using QPS.Domain.Entities.System;

namespace QPS.Application.Features.System;

public interface IErrorLogService
{
    Task LogErrorAsync(
        Exception exception,
        string requestUrl,
        string requestMethod,
        string requestBody,
        string userId,
        string username,
        string ipAddress,
        string userAgent,
        int httpStatusCode);
}

public class ErrorLogService : IErrorLogService
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ErrorLogService(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task LogErrorAsync(
        Exception exception,
        string requestUrl,
        string requestMethod,
        string requestBody,
        string userId,
        string username,
        string ipAddress,
        string userAgent,
        int httpStatusCode)
    {
        try
        {
            var errorLog = new SystemErrorLog(
                exception.GetType().FullName ?? "Unknown",
                exception.Message,
                exception.StackTrace ?? string.Empty,
                requestUrl,
                requestMethod,
                requestBody,
                userId,
                username,
                ipAddress,
                userAgent,
                httpStatusCode
            );

            errorLog.GetType().GetProperty("MerchantId")?.SetValue(errorLog, _currentUserService.MerchantId);

            await _dbContext.SystemErrorLogs.AddAsync(errorLog);
            await _dbContext.SaveChangesAsync();
        }
        catch
        {
        }
    }
}