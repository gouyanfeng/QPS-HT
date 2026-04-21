using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QPS.Application.Interfaces;

namespace QPS.Infrastructure.Identity;

public class JwtGenerator : IJwtGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtGenerator(string secretKey, string issuer, string audience)
    {
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _audience = audience ?? throw new ArgumentNullException(nameof(audience));
    }

    public string GenerateToken(Guid userId, Guid merchantId, string role, Guid? shopId = null)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim("merchantId", merchantId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        if (shopId.HasValue)
        {
            claims.Add(new Claim("shopId", shopId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}