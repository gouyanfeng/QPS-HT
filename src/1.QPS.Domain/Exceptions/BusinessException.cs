namespace QPS.Domain.Exceptions;

public class BusinessException : Exception
{
    public int ErrorCode { get; set; }

    public BusinessException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(int errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}