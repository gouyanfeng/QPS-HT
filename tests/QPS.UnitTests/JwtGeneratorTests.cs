using QPS.Application.Interfaces;
using QPS.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace QPS.UnitTests;

public class JwtGeneratorTests
{
    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // Arrange
        var secretKey = "test-secret-key-for-jwt-generation";
        var issuer = "test-issuer";
        var audience = "test-audience";
        var userId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var role = "Admin";
        var shopId = Guid.NewGuid();
        
        var jwtGenerator = new JwtGenerator(secretKey, issuer, audience);
        
        // Act
        var token = jwtGenerator.GenerateToken(userId, merchantId, role, shopId);
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Validate token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        Assert.Equal(issuer, jwtToken.Issuer);
        Assert.Equal(audience, jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, c => c.Type == "userId" && c.Value == userId.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == "merchantId" && c.Value == merchantId.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == role);
        Assert.Contains(jwtToken.Claims, c => c.Type == "shopId" && c.Value == shopId.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidTokenWithoutShopId()
    {
        // Arrange
        var secretKey = "test-secret-key-for-jwt-generation";
        var issuer = "test-issuer";
        var audience = "test-audience";
        var userId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var role = "Admin";
        
        var jwtGenerator = new JwtGenerator(secretKey, issuer, audience);
        
        // Act
        var token = jwtGenerator.GenerateToken(userId, merchantId, role);
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Validate token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        Assert.Equal(issuer, jwtToken.Issuer);
        Assert.Equal(audience, jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, c => c.Type == "userId" && c.Value == userId.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == "merchantId" && c.Value == merchantId.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == role);
        Assert.DoesNotContain(jwtToken.Claims, c => c.Type == "shopId");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenSecretKeyIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtGenerator(null, "test-issuer", "test-audience"));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenIssuerIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtGenerator("test-secret-key", null, "test-audience"));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAudienceIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtGenerator("test-secret-key", "test-issuer", null));
    }
}