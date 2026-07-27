namespace QPS.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? RequestPath { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
