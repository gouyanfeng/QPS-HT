using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemErrorLog : BaseEntity
{
    public string ErrorType { get; private set; }
    public string ErrorMessage { get; private set; }
    public string StackTrace { get; private set; }
    public string RequestUrl { get; private set; }
    public string RequestMethod { get; private set; }
    public string RequestBody { get; private set; }
    public string UserId { get; private set; }
    public string Username { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public int HttpStatusCode { get; private set; }

    private SystemErrorLog() { }

    public SystemErrorLog(
        string errorType,
        string errorMessage,
        string stackTrace,
        string requestUrl,
        string requestMethod,
        string requestBody,
        string userId,
        string username,
        string ipAddress,
        string userAgent,
        int httpStatusCode)
    {
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
        RequestUrl = requestUrl;
        RequestMethod = requestMethod;
        RequestBody = requestBody;
        UserId = userId;
        Username = username;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        HttpStatusCode = httpStatusCode;
    }
}