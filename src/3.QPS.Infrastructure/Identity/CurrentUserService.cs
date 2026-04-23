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

    public string? UserId
    {
        get
        {
            // 从JWT Token中获取用户ID
            // 这里假设用户ID存储在Claims中
            return _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        }
    }

    public Guid MerchantId
    {
        get
        {
            // 从请求头中获取商户ID
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Merchant-Id", out var merchantIdStr) == true &&
                Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return merchantId;
            }

            // 如果请求头中没有，从JWT Token中获取
            // 这里假设商户ID存储在Claims中
            if (_httpContextAccessor.HttpContext?.User?.FindFirst("merchantId")?.Value is string merchantIdClaim &&
                Guid.TryParse(merchantIdClaim, out var merchantIdFromClaim))
            {
                return merchantIdFromClaim;
            }

            return Guid.Empty;
        }
    }

    public Guid? ShopId
    {
        get
        {
            // 从请求头中获取店铺ID
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Shop-Id", out var shopIdStr) == true &&
                Guid.TryParse(shopIdStr, out var shopId))
            {
                return shopId;
            }

            // 如果请求头中没有，从JWT Token中获取
            // 这里假设店铺ID存储在Claims中
            if (_httpContextAccessor.HttpContext?.User?.FindFirst("shopId")?.Value is string shopIdClaim &&
                Guid.TryParse(shopIdClaim, out var shopIdFromClaim))
            {
                return shopIdFromClaim;
            }

            return null;
        }
    }
}