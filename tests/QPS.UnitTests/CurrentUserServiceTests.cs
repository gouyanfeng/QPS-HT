using Microsoft.AspNetCore.Http;
using Moq;
using QPS.Application.Interfaces;
using QPS.Infrastructure.Identity;
using System.Security.Claims;
using Xunit;

namespace QPS.UnitTests;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_ShouldReturnUserIdFromClaims()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var httpContextAccessorMock = CreateHttpContextAccessorMock(userId: userId);
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.UserId;
        
        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void UserId_ShouldReturnNull_WhenNoUserIdInClaims()
    {
        // Arrange
        var httpContextAccessorMock = CreateHttpContextAccessorMock();
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.UserId;
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MerchantId_ShouldReturnMerchantIdFromHeaders()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var httpContextAccessorMock = CreateHttpContextAccessorMock(merchantId: merchantId);
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.MerchantId;
        
        // Assert
        Assert.Equal(merchantId, result);
    }

    [Fact]
    public void MerchantId_ShouldReturnMerchantIdFromClaims_WhenNotInHeaders()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var httpContextAccessorMock = CreateHttpContextAccessorMock(merchantIdClaim: merchantId.ToString());
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.MerchantId;
        
        // Assert
        Assert.Equal(merchantId, result);
    }

    [Fact]
    public void MerchantId_ShouldReturnEmptyGuid_WhenNoMerchantIdFound()
    {
        // Arrange
        var httpContextAccessorMock = CreateHttpContextAccessorMock();
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.MerchantId;
        
        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void ShopId_ShouldReturnShopIdFromHeaders()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var httpContextAccessorMock = CreateHttpContextAccessorMock(shopId: shopId);
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.ShopId;
        
        // Assert
        Assert.Equal(shopId, result);
    }

    [Fact]
    public void ShopId_ShouldReturnShopIdFromClaims_WhenNotInHeaders()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var httpContextAccessorMock = CreateHttpContextAccessorMock(shopIdClaim: shopId.ToString());
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.ShopId;
        
        // Assert
        Assert.Equal(shopId, result);
    }

    [Fact]
    public void ShopId_ShouldReturnNull_WhenNoShopIdFound()
    {
        // Arrange
        var httpContextAccessorMock = CreateHttpContextAccessorMock();
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);
        
        // Act
        var result = currentUserService.ShopId;
        
        // Assert
        Assert.Null(result);
    }

    private Mock<IHttpContextAccessor> CreateHttpContextAccessorMock(
        string userId = null,
        Guid? merchantId = null,
        string merchantIdClaim = null,
        Guid? shopId = null,
        string shopIdClaim = null)
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        var claims = new List<Claim>();
        
        if (merchantId.HasValue)
        {
            headers.Add("X-Merchant-Id", merchantId.Value.ToString());
        }
        
        if (shopId.HasValue)
        {
            headers.Add("X-Shop-Id", shopId.Value.ToString());
        }
        
        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim("sub", userId));
        }
        
        if (!string.IsNullOrEmpty(merchantIdClaim))
        {
            claims.Add(new Claim("merchantId", merchantIdClaim));
        }
        
        if (!string.IsNullOrEmpty(shopIdClaim))
        {
            claims.Add(new Claim("shopId", shopIdClaim));
        }
        
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        
        requestMock.Setup(r => r.Headers).Returns(headers);
        httpContextMock.Setup(hc => hc.Request).Returns(requestMock.Object);
        httpContextMock.Setup(hc => hc.User).Returns(principal);
        httpContextAccessorMock.Setup(hca => hca.HttpContext).Returns(httpContextMock.Object);
        
        return httpContextAccessorMock;
    }
}