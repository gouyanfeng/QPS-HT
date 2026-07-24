using Microsoft.AspNetCore.Http;
using QPS.Application.Interfaces;

namespace QPS.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;
}
